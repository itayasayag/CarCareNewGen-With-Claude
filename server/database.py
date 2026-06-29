import pymssql
from pydantic_settings import BaseSettings
from functools import lru_cache
from typing import Optional
import urllib.parse


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


def _parse_odbc_string(odbc: str) -> dict:
    """Parse a pyodbc-style connection string into a dict."""
    result = {}
    for part in odbc.split(";"):
        part = part.strip()
        if "=" in part:
            k, v = part.split("=", 1)
            result[k.strip().lower()] = v.strip()
    return result


def get_connection():
    settings = get_settings()

    if settings.database_url:
        # Parse the ODBC-style DATABASE_URL into pymssql params
        params = _parse_odbc_string(settings.database_url)
        server = params.get("server", "").replace("tcp:", "").split(",")[0]
        database = params.get("database", "")
        user = params.get("uid", "")
        password = params.get("pwd", "")
        return pymssql.connect(
            server=server,
            user=user,
            password=password,
            database=database,
            tds_version="7.4"
        )

    # Local dev: individual fields from .env
    if settings.db_user and settings.db_password:
        return pymssql.connect(
            server=settings.db_server,
            user=settings.db_user,
            password=settings.db_password,
            database=settings.db_name,
            tds_version="7.4"
        )

    # Windows Auth (local only) — pymssql doesn't support Windows Auth,
    # so fall back to a trusted connection via the env vars
    raise RuntimeError(
        "No database credentials found. Set DATABASE_URL or DB_SERVER/DB_USER/DB_PASSWORD in .env"
    )
