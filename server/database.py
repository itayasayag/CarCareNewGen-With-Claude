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

    # ── Other settings ────────────────────────────────────────────────────────
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


def get_connection():
    settings = get_settings()

    if settings.database_url:
        # Parse the ODBC-style DATABASE_URL into pymssql params
        params = _parse_odbc_string(settings.database_url)
        server, port = _split_server_port(params.get("server", ""))
        database = params.get("database", "")
        user = params.get("uid", "")
        password = params.get("pwd", "")
        return pymssql.connect(
            server=server,
            port=port,
            user=user,
            password=password,
            database=database,
            tds_version="7.4",
            login_timeout=LOGIN_TIMEOUT,
            timeout=QUERY_TIMEOUT,
        )

    # Local dev: individual fields from .env
    if settings.db_user and settings.db_password:
        server, port = _split_server_port(settings.db_server or "")
        return pymssql.connect(
            server=server,
            port=port,
            user=settings.db_user,
            password=settings.db_password,
            database=settings.db_name,
            tds_version="7.4",
            login_timeout=LOGIN_TIMEOUT,
            timeout=QUERY_TIMEOUT,
        )

    raise RuntimeError(
        "No database credentials found. "
        "Set DATABASE_URL or DB_SERVER/DB_USER/DB_PASSWORD in .env"
    )
