from fastapi import APIRouter, HTTPException
from database import get_connection
from models.schemas import UserCarCreate, UserCarUpdate, UserCarResponse, SendCarRequest

router = APIRouter(prefix="/api/usercars", tags=["User Cars"])


def row_to_usercar(r) -> dict:
    return {
        "license_plate": r[0], "nick_name": r[1], "current_km": r[2],
        "pic_url": r[3], "user_email": r[4], "is_verified": bool(r[5])
    }


@router.get("/", response_model=list[UserCarCreate])
def get_all_user_cars():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified FROM UserCar")
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
            "SELECT LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified "
            "FROM UserCar WHERE UserEmail = ?", email
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
        cursor.execute(
            "SELECT NickName FROM UserCar WHERE UserEmail = ? AND LicensePlate = ?",
            email, license_plate
        )
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
        cursor.execute(
            "INSERT INTO UserCar (LicensePlate, NickName, CurrentKM, PicURL, UserEmail, IsVerified) "
            "VALUES (?, ?, ?, ?, ?, ?)",
            car.license_plate, car.nick_name, car.current_km,
            car.pic_url, car.user_email, car.is_verified
        )
        conn.commit()
        return {"message": "Car added successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/{email}/{license_plate}")
def update_user_car(email: str, license_plate: int, car: UserCarUpdate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "UPDATE UserCar SET NickName=?, CurrentKM=?, PicURL=? "
            "WHERE UserEmail=? AND LicensePlate=?",
            car.nick_name, car.current_km, car.pic_url, email, license_plate
        )
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
        cursor.execute(
            "UPDATE UserCar SET IsVerified=0 WHERE UserEmail=? AND LicensePlate=?",
            email, license_plate
        )
        conn.commit()
        return {"message": "Car deactivated"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/verify/{email}/{license_plate}")
def verify_car(email: str, license_plate: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "UPDATE UserCar SET IsVerified=1 WHERE UserEmail=? AND LicensePlate=?",
            email, license_plate
        )
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
        cursor.execute(
            "UPDATE UserCar SET UserEmail=? WHERE UserEmail=? AND LicensePlate=?",
            req.new_email, req.old_email, req.license_plate
        )
        conn.commit()
        return {"message": "Car transferred successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
