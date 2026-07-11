"""
Reminder emails — CURRENTLY DISABLED.

Email sending is parked for now: reminders live in the web app only.
Two blockers to revisit later:
  1. Railway blocks outbound SMTP on Free/Hobby plans.
  2. Sending to arbitrary user emails needs a verified sending domain
     (e.g. via Resend) — pending the custom-domain decision.

send_reminder_email() is kept as a no-op so routers importing it keep working.
The original SMTP implementation is preserved verbatim in the comment block at
the bottom of this file; restore it (or swap in the Resend HTTPS version) when
a sending domain is ready.
"""
from datetime import datetime, timedelta


def create_ics_content(remind_date: datetime, summary: str, notes: str = "") -> str:
    """Kept available in case the .ics is reused when email is re-enabled."""
    start = remind_date + timedelta(hours=6)
    end = remind_date + timedelta(hours=12)
    return f"""BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//CarCare//Reminders//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
DTSTART:{start.strftime('%Y%m%dT%H%M%SZ')}
DTEND:{end.strftime('%Y%m%dT%H%M%SZ')}
SUMMARY:{summary}
DESCRIPTION:{notes}
END:VEVENT
END:VCALENDAR"""


def send_reminder_email(to_email, care_name, car_nickname, remind_date, notes=""):
    """No-op while email is disabled. Reminders are web-only for now."""
    # Intentionally does nothing. See the module docstring and the commented
    # SMTP implementation below to re-enable.
    return


# ============================================================================
# ORIGINAL SMTP IMPLEMENTATION (disabled) — kept for reference / re-enabling.
# ============================================================================
# import smtplib
# import os
# from email.mime.multipart import MIMEMultipart
# from email.mime.text import MIMEText
# from email.mime.base import MIMEBase
# from email import encoders
# from datetime import datetime, timedelta
# from database import get_settings
#
#
# def create_ics_content(remind_date: datetime, summary: str, notes: str = "") -> str:
#     start = remind_date + timedelta(hours=6)
#     end = remind_date + timedelta(hours=12)
#     return f"""BEGIN:VCALENDAR
# VERSION:2.0
# PRODID:-//CarCare//Reminders//EN
# CALSCALE:GREGORIAN
# BEGIN:VEVENT
# DTSTART:{start.strftime('%Y%m%dT%H%M%SZ')}
# DTEND:{end.strftime('%Y%m%dT%H%M%SZ')}
# SUMMARY:{summary}
# DESCRIPTION:{notes}
# END:VEVENT
# END:VCALENDAR"""
#
#
# def send_reminder_email(
#     to_email: str,
#     care_name: str,
#     car_nickname: str,
#     remind_date: datetime,
#     notes: str = ""
# ):
#     settings = get_settings()
#     from_address = settings.email_address
#
#     # Fail fast with a clear message if SMTP isn't configured, instead of a
#     # confusing login error deep in the traceback.
#     if not settings.email_password:
#         print("[email] EMAIL_PASSWORD is not set — skipping reminder email. "
#               "Set EMAIL_ADDRESS and EMAIL_PASSWORD (Gmail App Password) in Railway.")
#         return
#
#     subject = f"{care_name} לרכב: {car_nickname}"
#     body = "CarCare זוהי תזכורת מאפליקציית"
#
#     ics_content = create_ics_content(remind_date, subject, notes)
#     ics_path = "CarCareReminder.ics"
#
#     try:
#         with open(ics_path, "w", encoding="utf-8") as f:
#             f.write(ics_content)
#
#         msg = MIMEMultipart()
#         msg["From"] = from_address
#         msg["To"] = to_email
#         msg["Subject"] = subject
#         msg.attach(MIMEText(body, "plain", "utf-8"))
#
#         with open(ics_path, "rb") as f:
#             part = MIMEBase("application", "octet-stream")
#             part.set_payload(f.read())
#             encoders.encode_base64(part)
#             part.add_header("Content-Disposition", f'attachment; filename="CarCareReminder.ics"')
#             msg.attach(part)
#
#         with smtplib.SMTP("smtp.gmail.com", 587, timeout=8) as server:
#             server.starttls()
#             server.login(from_address, settings.email_password)
#             server.sendmail(from_address, to_email, msg.as_string())
#
#         print(f"[email] Reminder sent to {to_email} from {from_address}")
#
#     except smtplib.SMTPAuthenticationError:
#         print("[email] SMTP auth failed. Check EMAIL_ADDRESS and that "
#               "EMAIL_PASSWORD is a valid Gmail App Password (2-Step Verification "
#               "must be on, and use the 16-char app password with no spaces).")
#     except Exception as e:
#         print(f"[email] Failed to send reminder email: {e}")
#     finally:
#         if os.path.exists(ics_path):
#             os.remove(ics_path)
