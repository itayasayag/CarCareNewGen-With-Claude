import re
import json
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from starlette.middleware.base import BaseHTTPMiddleware
from starlette.requests import Request
from starlette.responses import Response
from routers import users, user_cars, log_record, reminder, upload
from routers.lookups import car_model_router, garage_router, care_type_router


def snake_to_camel(name: str) -> str:
    components = name.split('_')
    return components[0] + ''.join(x.title() for x in components[1:])


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
            return Response(
                content=body,
                status_code=response.status_code,
                headers=dict(response.headers),
                media_type="application/json"
            )
        return response


app = FastAPI(
    title="CarCare API",
    description="Backend API for CarCare - vehicle maintenance tracking app",
    version="2.0.0",
)

# ── CORS (must be before CamelCaseMiddleware) ─────────────────────────────────
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
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
