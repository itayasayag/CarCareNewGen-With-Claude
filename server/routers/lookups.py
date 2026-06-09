from fastapi import APIRouter, HTTPException
from database import get_connection

# ── Car Model ─────────────────────────────────────────────────────────────────
car_model_router = APIRouter(prefix="/api/carmodel", tags=["Car Model"])

@car_model_router.get("/{license_plate}")
def get_car_model(license_plate: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT LicensePlate, Manufacturer, Model, YearOfManufacture, SubModelCode "
            "FROM CarModel WHERE LicensePlate = ?", license_plate
        )
        rows = cursor.fetchall()
        return [
            {
                "license_plate": r[0], "manufacturer": r[1], "model": r[2],
                "year_of_manufacture": r[3], "sub_model_code": r[4]
            } for r in rows
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


# ── Garage ────────────────────────────────────────────────────────────────────
garage_router = APIRouter(prefix="/api/garages", tags=["Garages"])

@garage_router.get("/cities")
def get_cities():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT DISTINCT Yishuv FROM Garage ORDER BY Yishuv")
        return [r[0] for r in cursor.fetchall()]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@garage_router.get("/{city}")
def get_garages_by_city(city: str):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT Id, Shem_mosah, Sug_mosah, Ktovet, Yishuv, Telephone, Rate "
            "FROM Garage WHERE Yishuv = ?", city
        )
        return [
            {
                "id": r[0], "name": r[1], "type": r[2], "address": r[3],
                "city": r[4], "telephone": r[5], "rate": r[6]
            } for r in cursor.fetchall()
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@garage_router.get("/by-id/{garage_id}")
def get_garage_by_id(garage_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT Id, Shem_mosah, Sug_mosah, Ktovet, Yishuv, Telephone, Rate "
            "FROM Garage WHERE Id = ?", garage_id
        )
        rows = cursor.fetchall()
        return [
            {
                "id": r[0], "name": r[1], "type": r[2], "address": r[3],
                "city": r[4], "telephone": r[5], "rate": r[6]
            } for r in rows
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


# ── Care Type ─────────────────────────────────────────────────────────────────
care_type_router = APIRouter(prefix="/api/caretypes", tags=["Care Types"])

@care_type_router.get("/")
def get_all_care_types():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT CareID, CareName, RecDaysForRepeat, RecKMForRepeat FROM CareType")
        return [
            {
                "care_id": r[0], "care_name": r[1],
                "rec_days_for_repeat": r[2], "rec_km_for_repeat": r[3]
            } for r in cursor.fetchall()
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()

@care_type_router.get("/{care_id}")
def get_care_type_by_id(care_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT CareID, CareName, RecDaysForRepeat, RecKMForRepeat "
            "FROM CareType WHERE CareID = ?", care_id
        )
        rows = cursor.fetchall()
        if not rows:
            raise HTTPException(status_code=404, detail="Care type not found")
        return [
            {
                "care_id": r[0], "care_name": r[1],
                "rec_days_for_repeat": r[2], "rec_km_for_repeat": r[3]
            } for r in rows
        ]
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
