import re
import json
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
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


class CamelCaseMiddleware:
    """
    Pure ASGI middleware that rewrites snake_case JSON response keys to camelCase.

    Implemented at the raw ASGI level (not Starlette's BaseHTTPMiddleware) because
    BaseHTTPMiddleware wraps responses in a way that interferes with CORSMiddleware's
    preflight (OPTIONS) handling, stripping Access-Control-* headers. Pure ASGI
    middleware buffers the body without touching headers added by other middleware,
    so CORS keeps working.
    """

    def __init__(self, app):
        self.app = app

    async def __call__(self, scope, receive, send):
        if scope["type"] != "http":
            await self.app(scope, receive, send)
            return

        # Only transform actual responses, never preflight
        if scope.get("method") == "OPTIONS":
            await self.app(scope, receive, send)
            return

        response_start = {}
        body_chunks = []
        is_json = {"value": False}

        async def send_wrapper(message):
            if message["type"] == "http.response.start":
                response_start["message"] = message
                for k, v in message.get("headers", []):
                    if k.decode("latin-1").lower() == "content-type" and \
                            "application/json" in v.decode("latin-1").lower():
                        is_json["value"] = True
                # Defer sending until we have the full body
                return
            elif message["type"] == "http.response.body":
                body_chunks.append(message.get("body", b""))
                if message.get("more_body", False):
                    return
                # Last chunk — process and flush
                full_body = b"".join(body_chunks)
                start_msg = response_start["message"]
                headers = [
                    (k, v) for k, v in start_msg.get("headers", [])
                    if k.decode("latin-1").lower() != "content-length"
                ]
                if is_json["value"]:
                    try:
                        data = json.loads(full_body)
                        data = convert_keys_to_camel(data)
                        full_body = json.dumps(data, ensure_ascii=False).encode("utf-8")
                    except Exception:
                        pass
                headers.append((b"content-length", str(len(full_body)).encode("latin-1")))
                start_msg["headers"] = headers
                await send(start_msg)
                await send({
                    "type": "http.response.body",
                    "body": full_body,
                    "more_body": False,
                })
                return
            else:
                await send(message)

        await self.app(scope, receive, send_wrapper)


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

# ── Middleware ordering note ──────────────────────────────────────────────────
# Starlette's app.add_middleware() inserts each middleware at the FRONT of the
# stack, so the LAST one added is the OUTERMOST layer. CORSMiddleware must be
# outermost so it can answer preflight OPTIONS requests and attach the
# Access-Control-* headers to every response — including ones produced by the
# CamelCaseMiddleware below. Therefore CamelCase is added FIRST (inner) and
# CORS is added LAST (outer). Reversing this drops CORS headers on preflight.

# ── camelCase response middleware (inner) ─────────────────────────────────────
app.add_middleware(CamelCaseMiddleware)

# ── CORS (outer — must be added LAST) ─────────────────────────────────────────
# allow_origins=["*"] together with allow_credentials=True is invalid per the
# CORS spec, so we list explicit origins and keep credentials off.
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
        "version": "2.0.0",
        "build": "cors-asgi-fix-2"   # bump to confirm a fresh deploy is live
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
        from fastapi.responses import JSONResponse
        return JSONResponse(
            content={"database": "error", "detail": str(e)},
            status_code=500,
        )
