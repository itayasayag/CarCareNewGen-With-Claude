import pyodbc
from pydantic_settings import BaseSettings
from functools import lru_cache
from typing import Optional


class Settings(BaseSettings):
    # ── Option A: individual fields (used locally via .env) ──────────────────
    db_server: Optional[str] = None
    db_name: Optional[str] = None
    db_user: Optional[str] = None
    db_password: Optional[str] = None

    # ── Option B: full pyodbc connection string (used on Railway/cloud) ──────
    # Set DATABASE_URL in Railway's environment variables panel.
    # Example:
    #   DATABASE_URL=DRIVER={ODBC Driver 18 for SQL Server};SERVER=yourserver.database.windows.net;DATABASE=CarCare;UID=youruser;PWD=yourpass;Encrypt=yes;TrustServerCertificate=no
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


def get_connection():
    settings = get_settings()

    # Option B: full connection string supplied directly (Railway / Azure)
    if settings.database_url:
        return pyodbc.connect(settings.database_url)

    # Option A: individual fields (local development via .env or Windows Auth)
    if settings.db_user and settings.db_password:
        conn_str = (
            f"DRIVER={{ODBC Driver 17 for SQL Server}};"
            f"SERVER={settings.db_server};"
            f"DATABASE={settings.db_name};"
            f"UID={settings.db_user};"
            f"PWD={settings.db_password};"
        )
    else:
        # Windows Auth (no username/password) — local dev only
        conn_str = (
            f"DRIVER={{ODBC Driver 17 for SQL Server}};"
            f"SERVER={settings.db_server};"
            f"DATABASE={settings.db_name};"
            f"Trusted_Connection=yes;"
        )
    return pyodbc.connect(conn_str)
