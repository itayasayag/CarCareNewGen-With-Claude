using CarCare.BL;
using System.Net.Mail;
using System.Net;

namespace server.BL
{
    public class Reminder
    {
        int reminderID;
        DateTime remindDate;
        string notes;
        string email;
        int careID;
		int licensePlate;

		public int ReminderID { get => reminderID; set => reminderID = value; }
		public DateTime RemindDate { get => remindDate; set => remindDate = value; }
		public string Notes { get => notes; set => notes = value; }
		public string Email { get => email; set => email = value; }
		public int CareID { get => careID; set => careID = value; }
		public int LicensePlate { get => licensePlate; set => licensePlate = value; }

		public Reminder() { }

		public Reminder(int reminderID, DateTime remindDate, string notes, string email, int careID, int licensePlate)
		{
			this.ReminderID = reminderID;
			this.RemindDate = remindDate;
			this.Notes = notes;
			this.Email = email;
			this.CareID = careID;
			this.LicensePlate = licensePlate;
		}

 

		public List<ReminderWithCare> ReadReminderByEmail(string email)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadReminderByEmail(email);
		}

		public void SendReminder(Reminder r)
		{
            try
            {
                UserCar userCar = new UserCar();
                string nickname = userCar.ReadNickName(r.Email, r.LicensePlate);
                // Sender's email address
                string fromAddress = "carcarereminders@gmail.com";
                // Recipient's email address
                string toAddress = r.Email;
                // Password of your email address
                const string fromPassword = "ymgi pvcz zekl gyql";
                // Mail subject
                CareType care = new CareType();
                List<CareType> CareName = care.ReadCareName(r.CareID);
                string ReminderCareName = CareName[0].CareName;
                string subject = ReminderCareName + " לרכב: " + nickname;
                // Mail body
                string body = "CarCare זוהי תזכורת מאפליקציית";

                // Create the ICS file content
                string icsContent = CreateIcsFile(r, subject);

                // Save the ICS file
                string icsFilePath = "CarCareReminder.ics";
                File.WriteAllText(icsFilePath, icsContent);

                // Create a new SmtpClient instance
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromAddress, fromPassword),
                    EnableSsl = true,
                };

                // Create a new MailMessage instance
                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body,
                })
                {
                    // Attach the ICS file
                    Attachment attachment = new Attachment(icsFilePath);
                    message.Attachments.Add(attachment);

                    try
                    {
                        // Send the email
                        smtpClient.Send(message);
                        Console.WriteLine("Email sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to send email. Error message: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
        }

        public string CreateIcsFile(Reminder r, string Summary)
        {
            string theEvent =
    $@"BEGIN:VCALENDAR
VERSION:2.0
PRODID:-//CarCare//Reminders//EN
CALSCALE:GREGORIAN
BEGIN:VEVENT
DTSTART:{r.RemindDate.AddHours(6):yyyyMMddTHHmmssZ}
DTEND:{r.RemindDate.AddHours(12):yyyyMMddTHHmmssZ}
SUMMARY:{Summary}
DESCRIPTION:{r.notes} 
END:VEVENT
END:VCALENDAR
";

            return theEvent;
        }

        public int InserReminder()
		{
			DBservices dbs = new DBservices();

			return dbs.InserReminder(this);
		}

		public int UpdateReminder()
		{
			DBservices dbs = new DBservices();
			return dbs.UpdateReminder(this);

		}

		public int DeleteReminder()
		{
			DBservices dbs = new DBservices();
			return dbs.DeleteReminder(this.ReminderID);
		}

	}
}
