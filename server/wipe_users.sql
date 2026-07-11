-- ============================================================
-- One-time cleanup before switching to bcrypt-hashed passwords.
-- Existing rows store PLAINTEXT passwords that will never match a
-- bcrypt verify, so we clear the user table and let everyone
-- re-register. Run this once in the Azure SQL Query Editor.
--
-- Order matters because of foreign keys: remove dependent rows first.
-- Adjust/comment out tables you want to keep.
-- ============================================================

DELETE FROM Reminder;
DELETE FROM Log_Record;
DELETE FROM UserCar;
DELETE FROM Users;

-- Verify
SELECT COUNT(*) AS remaining_users FROM Users;
