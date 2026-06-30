import re
import json
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import Response
from routers import users, user_cars, log_record, reminder, upload
from routers.lookups import car_model_router, garage_router, care_type_router


# Legacy frontend uses uppercase acronyms: careID, currentKM, picURL
ACRONYMS = {"id": "ID", "km": "KM", "url": "URL"}

def snake_to_camel(name: str) -> str:
    components = name.split('_')
    return components[0] + ''.join(ACRONYMS.get(x, x.title()) for x in components[1:])


def convert_keys_to_camel(obj):
    if isinstance(obj, dict):
        return {snake_to_camel(k): convert_keys_to_camel(v) for k, v in obj.items()}
    elif isinstance(obj, list):
        return [convert_keys_to_camel(i) for i in obj]
    return obj


class CamelCaseMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request: Request, call_next):
        response = await call_next(request)
        content_type = response.headers.get("content-type", "")
        if "application/json" in content_type:
            body = b""
            async for chunk in response.body_iterator:
                body += chunk
            try:
                data = json.loads(body)
                data = convert_keys_to_camel(data)
                body = json.dumps(data, ensure_ascii=False).encode("utf-8")
            except Exception:
                pass
            headers = dict(response.headers)
            headers.pop("content-length", None)  # let Response recompute it
            return Response(
                content=body,
                status_code=response.status_code,
                headers=headers,
                media_type="application/json"
            )
        return response


app = FastAPI(
    title="CarCare API",
    description="Backend API for CarCare - vehicle maintenance tracking app",
    version="2.0.0",
    # Disable automatic trailing-slash redirects. FastAPI's 307 redirect
    # response carries no CORS headers, so a cross-origin call to a path whose
    # slash doesn't match the route is redirected and then blocked by the
    # browser as a CORS error. With this off, both /api/users and /api/users/
    # resolve directly without a redirect.
    redirect_slashes=False,
)

# ── CORS (must be before CamelCaseMiddleware) ─────────────────────────────────
# NOTE: allow_origins=["*"] together with allow_credentials=True is invalid per
# the CORS spec — browsers reject it and no Access-Control-Allow-Origin header
# is sent. We list the real origins explicitly and keep credentials off.
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "https://itayasayag.github.io",   # GitHub Pages frontend
        "http://localhost:5500",          # local dev (Live Server)
        "http://127.0.0.1:5500",
        "http://localhost:8000",
        "http://127.0.0.1:8000",
    ],
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ── camelCase response middleware ─────────────────────────────────────────────
app.add_middleware(CamelCaseMiddleware)

# ── Routers ───────────────────────────────────────────────────────────────────
app.include_router(users.router)
app.include_router(user_cars.router)
app.include_router(car_model_router)
app.include_router(garage_router)
app.include_router(care_type_router)
app.include_router(log_record.router)
app.include_router(reminder.router)
app.include_router(upload.router)


@app.get("/")
def root():
    return {
        "message": "CarCare API is running!",
        "docs": "/docs",
        "version": "2.0.0"
    }


# ── Health check that actually tests the DB connection ────────────────────────
# Hit this to confirm the backend can reach Azure SQL. Returns the real error
# message if the connection fails, instead of a misleading CORS error.
@app.get("/health/db")
def health_db():
    from database import get_connection
    try:
        conn = get_connection()
        cur = conn.cursor()
        cur.execute("SELECT 1")
        cur.fetchone()
        conn.close()
        return {"database": "connected"}
    except Exception as e:
        return Response(
            content=json.dumps({"database": "error", "detail": str(e)}),
            status_code=500,
            media_type="application/json",
            headers={"Access-Control-Allow-Origin": "*"},
        )
