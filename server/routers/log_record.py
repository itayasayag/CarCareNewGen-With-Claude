from fastapi import APIRouter, HTTPException
from database import get_connection
from models.schemas import LogRecordCreate, LogRecordUpdate

router = APIRouter(prefix="/api/logs", tags=["Log Records"])


def row_to_log(r) -> dict:
    return {
        "log_id": r[0], "current_km": r[1], "record_date": r[2],
        "cost": r[3], "notes": r[4],
        "garage_id": r[5], "care_id": r[6], "user_email": r[7],
        "license_plate": r[8], "invoice_file_name": r[9],
        "care_name": r[10] if len(r) > 10 else None,
        "garage_name": r[11] if len(r) > 11 else None,
    }


@router.get("/{email}")
def get_logs_by_email(email: str):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            """SELECT lr.LogID, lr.CurrentKM, lr.RecordDate,
                      lr.Cost, lr.Notes, lr.Mispar_mosah, lr.CareID, lr.UserEmail,
                      lr.LicensePlate, lr.InvoiceFileName, ct.CareName,
                      COALESCE(lr.GarageName, g.Shem_mosah) AS GarageName
               FROM Log_Record lr
               LEFT JOIN CareType ct ON lr.CareID = ct.CareID
               LEFT JOIN Garage g ON lr.Mispar_mosah = g.Id
               WHERE lr.UserEmail = ?""",
            email
        )
        return [row_to_log(r) for r in cursor.fetchall()]
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.get("/by-id/{log_id}")
def get_log_by_id(log_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            """SELECT lr.LogID, lr.CurrentKM, lr.RecordDate,
                      lr.Cost, lr.Notes, lr.Mispar_mosah, lr.CareID, lr.UserEmail,
                      lr.LicensePlate, lr.InvoiceFileName, ct.CareName,
                      COALESCE(lr.GarageName, g.Shem_mosah) AS GarageName
               FROM Log_Record lr
               LEFT JOIN CareType ct ON lr.CareID = ct.CareID
               LEFT JOIN Garage g ON lr.Mispar_mosah = g.Id
               WHERE lr.LogID = ?""",
            log_id
        )
        rows = cursor.fetchall()
        if not rows:
            raise HTTPException(status_code=404, detail="Log not found")
        return [row_to_log(r) for r in rows]
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.post("/", status_code=201)
def create_log(log: LogRecordCreate):
    try:
        conn = get_connection()
        cursor = conn.cursor()

        # Garages now come from the gov.il API, so their IDs won't exist in the
        # local Garage table. Only keep garage_id if it actually exists locally;
        # otherwise store NULL to avoid a foreign-key violation.
        garage_id = log.garage_id
        if garage_id is not None:
            cursor.execute("SELECT 1 FROM Garage WHERE Id = ?", garage_id)
            if cursor.fetchone() is None:
                garage_id = None

        cursor.execute(
            """INSERT INTO Log_Record
               (CurrentKM, RecordDate, Cost, Notes,
                Mispar_mosah, CareID, UserEmail, LicensePlate, InvoiceFileName, GarageName)
               VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""",
            log.current_km, log.record_date,
            log.cost, log.notes, garage_id, log.care_id,
            log.user_email, log.license_plate, log.invoice_file_name,
            log.garage_name
        )
        conn.commit()
        return {"message": "Log record created successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.put("/{log_id}")
def update_log(log_id: int, log: LogRecordUpdate):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute(
            """UPDATE Log_Record
               SET CurrentKM=?, RecordDate=?,
                   Cost=?, Notes=?, Mispar_mosah=?, CareID=?, InvoiceFileName=?
               WHERE LogID=?""",
            log.current_km, log.record_date,
            log.cost, log.notes, log.garage_id, log.care_id,
            log.invoice_file_name, log_id
        )
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Log not found")
        return {"message": "Log updated successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()


@router.delete("/{log_id}")
def delete_log(log_id: int):
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("DELETE FROM Log_Record WHERE LogID = ?", log_id)
        conn.commit()
        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Log not found")
        return {"message": "Log deleted successfully"}
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        conn.close()
