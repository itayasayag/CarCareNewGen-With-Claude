from fastapi import APIRouter, HTTPException, BackgroundTasks
from database import get_connection
from models.schemas import ReminderCreate, ReminderUpdate
from services.email_service import send_reminder_email

router = APIRouter(prefix="/api/reminders", tags=["Reminders"])


def get_care_name(cursor, care_id: int) -> str:
    cursor.execute("SELECT CareName FROM CareType WHERE CareID = %s", care_id)
    row = cursor.fetchone()
    return row[0] if row else ""


def get_car_nickname(cursor, email: str, license_plate: int) -> str:
    cursor.execute("SELECT NickName FROM UserCar WHERE UserEmail = %s AND LicensePlate = %s", (email, license_plate))
    row = cursor.fetchone()
    return row[0] if row else ""


@router.get("/{email}")
def get_reminders_by_email(email: str):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            """SELECT r.ReminderID, r.RemindDate, r.Notes, r.CareID, r.Email,
                      r.LicensePlate, ct.CareName, uc.NickName
               FROM Reminder r
               LEFT JOIN CareType ct ON r.CareID = ct.CareID
               LEFT JOIN UserCar uc ON r.Email = uc.UserEmail AND r.LicensePlate = uc.LicensePlate
               WHERE r.Email = %s""",
            email
        )
        return [
            {
                "reminder_id": r[0], "remind_date": r[1], "notes": r[2],
                "care_id": r[3], "email": r[4], "license_plate": r[5],
                "care_name": r[6], "car_nickname": r[7]
            } for r in cursor.fetchall()
        ]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/", status_code=201)
def create_reminder(reminder: ReminderCreate, background_tasks: BackgroundTasks):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        care_name = get_care_name(cursor, reminder.care_id)
        car_nickname = get_car_nickname(cursor, reminder.email, reminder.license_plate)

        cursor.execute("INSERT INTO Reminder (RemindDate, Notes, Email, CareID, LicensePlate) "
            "VALUES (%s, %s, %s, %s, %s)", (reminder.remind_date, reminder.notes, reminder.email, reminder.care_id, reminder.license_plate))
        conn.commit()

        # ── Email reminders DISABLED for now ──────────────────────────────────
        # Reminders currently live in the web app only. Email sending is parked
        # until a verified sending domain is set up (Railway blocks SMTP, and
        # Resend needs a verified domain to send to arbitrary user emails).
        # To re-enable: uncomment the block below (and see services/email_service.py).
        #
        # background_tasks.add_task(
        #     send_reminder_email,
        #     to_email=reminder.email,
        #     care_name=care_name,
        #     car_nickname=car_nickname,
        #     remind_date=reminder.remind_date,
        #     notes=reminder.notes or ""
        # )

        return {"message": "Reminder created successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/{reminder_id}")
def update_reminder(reminder_id: int, reminder: ReminderUpdate, background_tasks: BackgroundTasks):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        # Fetch current reminder for email sending
        cursor.execute(
            "SELECT Email, LicensePlate FROM Reminder WHERE ReminderID = %s", reminder_id
        )
        row = cursor.fetchone()
        if not row:
            raise HTTPException(status_code=404, detail="Reminder not found")

        email, license_plate = row
        care_name = get_care_name(cursor, reminder.care_id) if reminder.care_id else ""
        car_nickname = get_car_nickname(cursor, email, license_plate)

        cursor.execute("UPDATE Reminder SET RemindDate=%s, Notes=%s, CareID=%s WHERE ReminderID=%s", (reminder.remind_date, reminder.notes, reminder.care_id, reminder_id))
        conn.commit()

        # ── Email reminders DISABLED for now (see create_reminder note) ───────
        # background_tasks.add_task(
        #     send_reminder_email,
        #     to_email=email,
        #     care_name=care_name,
        #     car_nickname=car_nickname,
        #     remind_date=reminder.remind_date,
        #     notes=reminder.notes or ""
        # )

        return {"message": "Reminder updated successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.delete("/{reminder_id}")
def delete_reminder(reminder_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("DELETE FROM Reminder WHERE ReminderID = %s", reminder_id)
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Reminder not found")
        return {"message": "Reminder deleted successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
