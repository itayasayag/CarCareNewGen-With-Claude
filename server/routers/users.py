from fastapi import APIRouter, HTTPException
from database import get_connection
from models.schemas import UserCreate, UserUpdate, UserLogin, UserResponse, PasswordReset
from services.password_service import hash_password, verify_password

router = APIRouter(prefix="/api/users", tags=["Users"])


@router.post("/reset-password")
def reset_password(data: PasswordReset):
    """
    Simple self-service password reset by email.

    NOTE: with no email-verification infrastructure, this trusts that whoever
    submits the form owns the email. That's acceptable for this app's current
    (non-public, tiny) user base, but is NOT secure for real users — a proper
    version would email a one-time reset link/token. Flagged intentionally.
    """
    if not data.new_password or len(data.new_password) < 4:
        raise HTTPException(status_code=400, detail="Password too short")
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("UPDATE Users SET Password=%s WHERE Email=%s",
                       (hash_password(data.new_password), data.email))
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="No account with that email")
        return {"message": "Password reset successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


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
        # Store a one-way bcrypt hash, never the plaintext password.
        hashed = hash_password(user.password)
        cursor.execute("INSERT INTO Users (Email, FirstName, FamilyName, Password, PhoneNum, Verified) "
            "VALUES (%s, %s, %s, %s, %s, 0)", (user.email, user.first_name, user.family_name, hashed, user.phone_num))
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
            "SELECT Email, FirstName, FamilyName, PhoneNum, Verified FROM Users WHERE Email = %s",
            user.email
        )
        row = cursor.fetchone()
        if row:
            return UserResponse(
                email=row[0], first_name=row[1], family_name=row[2],
                phone_num=row[3], verified=bool(row[4])
            )
        # New Google user — create them (Google accounts are considered verified)
        cursor.execute("INSERT INTO Users (Email, FirstName, FamilyName, Password, PhoneNum, Verified) "
            "VALUES (%s, %s, %s, %s, %s, 1)", (user.email, user.first_name, user.family_name, "", user.phone_num or ""))
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
        # Fetch the stored hash by email, then verify in code. We no longer
        # compare passwords in SQL (that required storing plaintext).
        cursor.execute("SELECT Email, FirstName, FamilyName, PhoneNum, Verified, Password FROM Users "
            "WHERE Email = %s", credentials.email)
        row = cursor.fetchone()
        if not row or not verify_password(credentials.password, row[5]):
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
        # Build the update dynamically so a profile edit that doesn't include a
        # password (e.g. the phone-number update) never blanks the stored hash.
        sets, params = [], []
        if user.first_name is not None:
            sets.append("FirstName=%s"); params.append(user.first_name)
        if user.family_name is not None:
            sets.append("FamilyName=%s"); params.append(user.family_name)
        if user.phone_num is not None:
            sets.append("PhoneNum=%s"); params.append(user.phone_num)
        if user.password:  # only when a non-empty new password is supplied
            sets.append("Password=%s"); params.append(hash_password(user.password))

        if not sets:
            return {"message": "Nothing to update"}

        params.append(email)
        cursor.execute("UPDATE Users SET " + ", ".join(sets) + " WHERE Email=%s", tuple(params))
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
        cursor.execute("DELETE FROM Users WHERE Email = %s", email)
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
