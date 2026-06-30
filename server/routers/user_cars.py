from fastapi import APIRouter, HTTPException
from database import get_connection
from models.schemas import UserCarCreate, UserCarUpdate, UserCarResponse, SendCarRequest

router = APIRouter(prefix="/api/usercars", tags=["User Cars"])


def row_to_usercar(r) -> dict:
    return {
        "license_plate": r[0], "nick_name": r[1], "current_km": r[2],
        "pic_url": r[3], "user_email": r[4], "is_verified": bool(r[5]),
        "is_active": bool(r[6])
    }


@router.get("/", response_model=list[UserCarCreate])
def get_all_user_cars():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified, IsActive "
            "FROM UserCar WHERE IsActive = 1"
        )
        return [row_to_usercar(r) for r in cursor.fetchall()]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.get("/{email}", response_model=list[UserCarCreate])
def get_user_cars_by_email(email: str):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified, IsActive "
            "FROM UserCar WHERE UserEmail = %s AND IsActive = 1", email
        )
        return [row_to_usercar(r) for r in cursor.fetchall()]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.get("/nickname/{email}/{license_plate}")
def get_nickname(email: str, license_plate: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT NickName FROM UserCar WHERE UserEmail = %s AND LicensePlate = %s", (email, license_plate))
        row = cursor.fetchone()
        if not row:
            raise HTTPException(status_code=404, detail="Car not found")
        return {"nickname": row[0]}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/", status_code=201)
def add_user_car(car: UserCarCreate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT IsActive FROM UserCar WHERE UserEmail=%s AND LicensePlate=%s", (car.user_email, car.license_plate))
        existing = cursor.fetchone()
        if existing is not None:
            if existing[0]:
                raise HTTPException(status_code=409, detail="Car already exists for this user")
            # Previously detached — reactivate instead of inserting a duplicate row.
            cursor.execute("UPDATE UserCar SET IsActive=1, NickName=%s, CurrentKM=%s, PicURL=%s "
                "WHERE UserEmail=%s AND LicensePlate=%s", (car.nick_name, car.current_km, car.pic_url, car.user_email, car.license_plate))
            conn.commit()
            return {"message": "Car reactivated successfully"}

        cursor.execute("INSERT INTO UserCar (LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified, IsActive) "
            "VALUES (%s, %s, %s, %s, %s, %s, 1)", (car.license_plate, car.nick_name, car.current_km, car.pic_url, car.user_email, car.is_verified))
        conn.commit()
        return {"message": "Car added successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/{email}/{license_plate}")
def update_user_car(email: str, license_plate: int, car: UserCarUpdate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE UserCar SET NickName=%s, CurrentKM=%s, PicURL=%s "
            "WHERE UserEmail=%s AND LicensePlate=%s", (car.nick_name, car.current_km, car.pic_url, email, license_plate))
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Car not found")
        return {"message": "Car updated successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/deactivate/{email}/{license_plate}")
def deactivate_car(email: str, license_plate: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE UserCar SET IsActive=0 WHERE UserEmail=%s AND LicensePlate=%s", (email, license_plate))
        conn.commit()
        return {"message": "Car removed from your garage"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/verify/{email}/{license_plate}")
def verify_car(email: str, license_plate: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE UserCar SET IsVerified=1 WHERE UserEmail=%s AND LicensePlate=%s", (email, license_plate))
        conn.commit()
        return {"message": "Car verified"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/transfer")
def transfer_car(req: SendCarRequest):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE UserCar SET UserEmail=%s WHERE UserEmail=%s AND LicensePlate=%s", (req.new_email, req.old_email, req.license_plate))
        conn.commit()
        return {"message": "Car transferred successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
