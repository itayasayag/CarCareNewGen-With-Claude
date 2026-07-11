import pymssql
from pydantic_settings import BaseSettings
from functools import lru_cache
from typing import Optional


class Settings(BaseSettings):
    # ── Option A: individual fields (local dev via .env) ─────────────────────
    db_server: Optional[str] = None
    db_name: Optional[str] = None
    db_user: Optional[str] = None
    db_password: Optional[str] = None

    # ── Option B: full connection string (Railway / Azure) ───────────────────
    # Format: SERVER=x;DATABASE=y;UID=z;PWD=w;Encrypt=yes;...
    database_url: Optional[str] = None

    # ── Email (SMTP) ──────────────────────────────────────────────────────────
    # Set EMAIL_ADDRESS and EMAIL_PASSWORD as environment variables (e.g. in
    # Railway). EMAIL_PASSWORD must be a Gmail *App Password* (16 chars, no
    # spaces), not the account's normal password. These override the defaults.
    email_address: str = "carcarereminders@gmail.com"
    email_password: str = ""
    upload_dir: str = "uploadedFiles"

    class Config:
        env_file = ".env"


@lru_cache()
def get_settings() -> Settings:
    return Settings()


# ── Connection tuning ─────────────────────────────────────────────────────────
# Azure SQL (especially Basic tier) can be slow to wake up / accept the first
# connection. Generous timeouts prevent spurious "connection failed" errors that
# the browser would otherwise surface as a CORS / network failure.
LOGIN_TIMEOUT = 30   # seconds to establish the initial login
QUERY_TIMEOUT = 30   # seconds to wait for a query to return
DEFAULT_PORT = 1433


def _parse_odbc_string(odbc: str) -> dict:
    """Parse a pyodbc-style connection string into a lowercase-keyed dict."""
    result = {}
    for part in odbc.split(";"):
        part = part.strip()
        if "=" in part:
            k, v = part.split("=", 1)
            result[k.strip().lower()] = v.strip()
    return result


def _split_server_port(raw_server: str):
    """
    Turn 'tcp:carcaresqldb.database.windows.net,1433' into
    ('carcaresqldb.database.windows.net', 1433).
    """
    server = raw_server.replace("tcp:", "").strip()
    port = DEFAULT_PORT
    if "," in server:
        server, port_str = server.split(",", 1)
        try:
            port = int(port_str.strip())
        except ValueError:
            port = DEFAULT_PORT
    return server.strip(), port


def _connect_params():
    """Resolve pymssql connect kwargs from settings (DATABASE_URL or fields)."""
    settings = get_settings()
    if settings.database_url:
        params = _parse_odbc_string(settings.database_url)
        server, port = _split_server_port(params.get("server", ""))
        return dict(
            server=server, port=port,
            user=params.get("uid", ""),
            password=params.get("pwd", ""),
            database=params.get("database", ""),
        )
    if settings.db_user and settings.db_password:
        server, port = _split_server_port(settings.db_server or "")
        return dict(
            server=server, port=port,
            user=settings.db_user,
            password=settings.db_password,
            database=settings.db_name,
        )
    raise RuntimeError(
        "No database credentials found. "
        "Set DATABASE_URL or DB_SERVER/DB_USER/DB_PASSWORD in .env"
    )


def _new_raw_connection():
    kwargs = _connect_params()
    return pymssql.connect(
        tds_version="7.4",
        login_timeout=LOGIN_TIMEOUT,
        timeout=QUERY_TIMEOUT,
        **kwargs,
    )


# ── Connection pool ───────────────────────────────────────────────────────────
# Opening a fresh connection to Azure SQL from Railway (a cross-region hop)
# costs a full TLS + login handshake — several round trips, ~0.5-1s each time.
# Doing that on EVERY request is the main source of slow responses. The pool
# keeps a set of live connections and reuses them, so the handshake happens
# rarely instead of per request.
#
# Endpoints still call get_connection() and conn.close() exactly as before —
# but close() now returns the connection to the pool instead of tearing it down.
import threading
import queue

POOL_SIZE = 5
_pool: "queue.Queue" = queue.Queue(maxsize=POOL_SIZE)
_pool_lock = threading.Lock()
_pool_initialized = False


class PooledConnection:
    """
    Transparent proxy around a pymssql connection.

    Delegates everything to the real connection, except .close(), which returns
    the connection to the pool for reuse instead of closing it. If the connection
    is dead, it's discarded and a fresh one is created on next use.
    """
    def __init__(self, raw):
        self._raw = raw

    def __getattr__(self, name):
        # Delegate cursor(), commit(), rollback(), etc. to the real connection
        return getattr(self._raw, name)

    def close(self):
        # Return to pool instead of actually closing
        _return_to_pool(self._raw)

    def _hard_close(self):
        try:
            self._raw.close()
        except Exception:
            pass


def _return_to_pool(raw):
    try:
        # Roll back any uncommitted state so the next user gets a clean session
        try:
            raw.rollback()
        except Exception:
            pass
        _pool.put_nowait(raw)
    except queue.Full:
        # Pool is full — close this extra connection for real
        try:
            raw.close()
        except Exception:
            pass


def get_connection():
    """
    Return a pooled connection (as a PooledConnection proxy). Reuses an idle
    connection if available, otherwise opens a new one. Callers use it and call
    .close() as usual; close() returns it to the pool.
    """
    raw = None
    try:
        raw = _pool.get_nowait()
        # Validate the pooled connection is still alive; if not, replace it
        try:
            cur = raw.cursor()
            cur.execute("SELECT 1")
            cur.fetchone()
        except Exception:
            try:
                raw.close()
            except Exception:
                pass
            raw = _new_raw_connection()
    except queue.Empty:
        raw = _new_raw_connection()
    return PooledConnection(raw)
