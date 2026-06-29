import urllib.parse
from functools import lru_cache
from typing import Optional

import pymssql
from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    db_server: Optional[str] = None
    db_name: Optional[str] = None
    db_user: Optional[str] = None
    db_password: Optional[str] = None
    database_url: Optional[str] = None

    email_address: str = "carcarereminders@gmail.com"
    email_password: str = ""
    upload_dir: str = "uploadedFiles"

    class Config:
        env_file = ".env"
        extra = "ignore"


@lru_cache()
def get_settings() -> Settings:
    return Settings()


def _parse_odbc_string(value: str) -> dict:
    result = {}
    for part in value.split(";"):
        if "=" in part:
            key, val = part.split("=", 1)
            result[key.strip().lower()] = val.strip()
    return result


def _parse_database_url(value: str) -> dict:
    value = value.strip()

    if "://" in value:
        parsed = urllib.parse.urlparse(value)
        return {
            "server": parsed.hostname or "",
            "port": parsed.port,
            "database": parsed.path.lstrip("/"),
            "user": urllib.parse.unquote(parsed.username or ""),
            "password": urllib.parse.unquote(parsed.password or ""),
        }

    params = _parse_odbc_string(value)
    server = params.get("server") or params.get("data source") or ""
    database = params.get("database") or params.get("initial catalog") or ""
    user = params.get("uid") or params.get("user id") or params.get("user") or ""
    password = params.get("pwd") or params.get("password") or ""
    port = params.get("port")

    server = server.replace("tcp:", "")
    if "," in server:
        server, parsed_port = server.rsplit(",", 1)
        port = port or parsed_port

    return {
        "server": server,
        "port": int(port) if str(port or "").isdigit() else None,
        "database": database,
        "user": user,
        "password": password,
    }


class PymssqlCursor:
    def __init__(self, cursor):
        self.cursor = cursor

    def execute(self, query, *params):
        query = query.replace("?", "%s")
        if not params:
            return self.cursor.execute(query)
        if len(params) == 1:
            param = params[0]
            if isinstance(param, (list, tuple)):
                return self.cursor.execute(query, tuple(param))
            return self.cursor.execute(query, (param,))
        return self.cursor.execute(query, params)

    def __getattr__(self, name):
        return getattr(self.cursor, name)


class PymssqlConnection:
    def __init__(self, conn):
        self.conn = conn

    def cursor(self, *args, **kwargs):
        return PymssqlCursor(self.conn.cursor(*args, **kwargs))

    def __getattr__(self, name):
        return getattr(self.conn, name)


def _connect(server, user, password, database, port=None):
    if not all([server, user, password, database]):
        raise RuntimeError("Database credentials are incomplete.")

    kwargs = {
        "server": server,
        "user": user,
        "password": password,
        "database": database,
        "tds_version": "7.4",
    }
    if port:
        kwargs["port"] = port

    return PymssqlConnection(pymssql.connect(**kwargs))


def get_connection():
    settings = get_settings()

    if settings.database_url:
        params = _parse_database_url(settings.database_url)
        return _connect(
            params["server"],
            params["user"],
            params["password"],
            params["database"],
            params["port"],
        )

    if settings.db_user and settings.db_password:
        return _connect(
            settings.db_server,
            settings.db_user,
            settings.db_password,
            settings.db_name,
        )

    raise RuntimeError("No database credentials found.")