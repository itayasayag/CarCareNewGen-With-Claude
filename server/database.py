import pyodbc
from pydantic_settings import BaseSettings
from functools import lru_cache


class Settings(BaseSettings):
    db_server: str
    db_name: str
    db_user: str
    db_password: str
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
    conn_str = (
        f"DRIVER={{ODBC Driver 17 for SQL Server}};"
        f"SERVER={settings.db_server};"
        f"DATABASE={settings.db_name};"
        f"UID={settings.db_user};"
        f"PWD={settings.db_password};"
    )
    return pyodbc.connect(conn_str)
