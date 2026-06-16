from fastapi import APIRouter, HTTPException
from database import get_connection
from models.schemas import UserCreate, UserUpdate, UserLogin, UserResponse

router = APIRouter(prefix="/api/users", tags=["Users"])


@router.get("/", response_model=list[UserResponse])
def get_all_users():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT Email, FirstName, FamilyName, PhoneNum, Verified FROM Users")
        rows = cursor.fetchall()
        return [
            UserResponse(
                email=r[0], first_name=r[1], family_name=r[2],
                phone_num=r[3], verified=bool(r[4])
            ) for r in rows
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/", status_code=201)
def register_user(user: UserCreate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "INSERT INTO Users (Email, FirstName, FamilyName, Password, PhoneNum, Verified) "
            "VALUES (?, ?, ?, ?, ?, 0)",
            user.email, user.first_name, user.family_name, user.password, user.phone_num
        )
        conn.commit()
        return {"message": "User registered successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/google-login", response_model=UserResponse)
def google_login(user: UserCreate):
    """Login or auto-register a user authenticated via Google.
    Looks up by email; creates the user (no password) if they don't exist yet."""
    try:
        conn = get_connection()
        cursor = conn.cursor()
        # Does this email already exist?
        cursor.execute(
            "SELECT Email, FirstName, FamilyName, PhoneNum, Verified FROM Users WHERE Email = ?",
            user.email
        )
        row = cursor.fetchone()
        if row:
            return UserResponse(
                email=row[0], first_name=row[1], family_name=row[2],
                phone_num=row[3], verified=bool(row[4])
            )
        # New Google user — create them (Google accounts are considered verified)
        cursor.execute(
            "INSERT INTO Users (Email, FirstName, FamilyName, Password, PhoneNum, Verified) "
            "VALUES (?, ?, ?, ?, ?, 1)",
            user.email, user.first_name, user.family_name, "", (user.phone_num or "")
        )
        conn.commit()
        return UserResponse(
            email=user.email, first_name=user.first_name,
            family_name=user.family_name, phone_num=(user.phone_num or ""), verified=True
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/login", response_model=UserResponse)
def login(credentials: UserLogin):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "SELECT Email, FirstName, FamilyName, PhoneNum, Verified FROM Users "
            "WHERE Email = ? AND Password = ?",
            credentials.email, credentials.password
        )
        row = cursor.fetchone()
        if not row:
            raise HTTPException(status_code=401, detail="Invalid email or password")
        return UserResponse(
            email=row[0], first_name=row[1], family_name=row[2],
            phone_num=row[3], verified=bool(row[4])
        )
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/{email}")
def update_user(email: str, user: UserUpdate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            "UPDATE Users SET FirstName=?, FamilyName=?, PhoneNum=?, Password=? WHERE Email=?",
            user.first_name, user.family_name, user.phone_num, user.password, email
        )
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="User not found")
        return {"message": "User updated successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.delete("/{email}")
def delete_user(email: str):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("DELETE FROM Users WHERE Email = ?", email)
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="User not found")
        return {"message": "User deleted successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
