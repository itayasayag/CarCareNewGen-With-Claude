from pydantic import BaseModel, ConfigDict
from typing import Optional
from datetime import datetime

# Legacy frontend uses uppercase acronyms: careID, currentKM, picURL
ACRONYMS = {"id": "ID", "km": "KM", "url": "URL"}

def to_legacy_camel(name: str) -> str:
    parts = name.split('_')
    return parts[0] + ''.join(ACRONYMS.get(p, p.title()) for p in parts[1:])


class CamelModel(BaseModel):
    model_config = ConfigDict(
        alias_generator=to_legacy_camel,
        populate_by_name=True,  # accept both snake_case and camelCase on input
        from_attributes=True
    )


# ── User ──────────────────────────────────────────────────────────────────────
class UserBase(CamelModel):
    email: str
    first_name: str
    family_name: str
    phone_num: Optional[str] = None

class UserCreate(UserBase):
    password: str

class UserUpdate(CamelModel):
    first_name: Optional[str] = None
    family_name: Optional[str] = None
    phone_num: Optional[str] = None
    password: Optional[str] = None

class UserLogin(CamelModel):
    email: str
    password: str

class UserResponse(UserBase):
    verified: bool = False


# ── UserCar ───────────────────────────────────────────────────────────────────
class UserCarBase(CamelModel):
    license_plate: int
    nick_name: Optional[str] = None
    current_km: Optional[int] = 0
    pic_url: Optional[str] = None
    user_email: str
    is_verified: bool = False
    is_active: bool = True

class UserCarCreate(UserCarBase):
    pass

class UserCarResponse(UserCarBase):
    pass

class UserCarUpdate(CamelModel):
    nick_name: Optional[str] = None
    current_km: Optional[int] = None
    pic_url: Optional[str] = None

class SendCarRequest(CamelModel):
    new_email: str
    old_email: str
    license_plate: int


# ── CarModel ──────────────────────────────────────────────────────────────────
class CarModelResponse(CamelModel):
    license_plate: int
    manufacturer: Optional[str] = None
    model: Optional[str] = None
    year_of_manufacture: Optional[int] = None
    sub_model_code: Optional[str] = None


# ── Garage ────────────────────────────────────────────────────────────────────
class GarageResponse(CamelModel):
    id: int
    name: str
    type: Optional[str] = None
    address: Optional[str] = None
    city: str
    telephone: Optional[str] = None
    rate: Optional[int] = None


# ── CareType ──────────────────────────────────────────────────────────────────
class CareTypeResponse(CamelModel):
    care_id: int
    care_name: str
    rec_days_for_repeat: Optional[int] = None
    rec_km_for_repeat: Optional[int] = None


# ── LogRecord ─────────────────────────────────────────────────────────────────
class LogRecordBase(CamelModel):
    current_km: int
    record_date: datetime
    warranty_expiration_date: Optional[datetime] = None
    cost: Optional[int] = None
    notes: Optional[str] = None
    garage_id: Optional[int] = None
    care_id: int
    user_email: str
    license_plate: int
    invoice_file_name: Optional[str] = None

class LogRecordCreate(LogRecordBase):
    pass

class LogRecordUpdate(CamelModel):
    current_km: Optional[int] = None
    record_date: Optional[datetime] = None
    warranty_expiration_date: Optional[datetime] = None
    cost: Optional[int] = None
    notes: Optional[str] = None
    garage_id: Optional[int] = None
    care_id: Optional[int] = None
    invoice_file_name: Optional[str] = None

class LogRecordResponse(LogRecordBase):
    log_id: int
    care_name: Optional[str] = None
    garage_name: Optional[str] = None


# ── Reminder ──────────────────────────────────────────────────────────────────
class ReminderBase(CamelModel):
    remind_date: datetime
    notes: Optional[str] = None
    email: str
    care_id: int
    license_plate: int

class ReminderCreate(ReminderBase):
    pass

class ReminderUpdate(CamelModel):
    remind_date: Optional[datetime] = None
    notes: Optional[str] = None
    care_id: Optional[int] = None

class ReminderResponse(ReminderBase):
    reminder_id: int
    care_name: Optional[str] = None
    car_nickname: Optional[str] = None
