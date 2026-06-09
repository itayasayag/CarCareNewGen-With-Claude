using Microsoft.Extensions.Hosting;

namespace server.BL
{
    public class Log_Record
    {
        int logID;
        int currentKM;
        DateTime recordDate;
        DateTime warrantyExpirationDate;
        int cost;
        string notes;
        int mispar_mosah;
        int careID;
		string useremail;
		int licensePlate;
		string invoiceFileName;
		string carename;
		string garagename;

		public int LogID { get => logID; set => logID = value; }
		public int CurrentKM { get => currentKM; set => currentKM = value; }
		public DateTime RecordDate { get => recordDate; set => recordDate = value; }
		public DateTime WarrantyExpirationDate { get => warrantyExpirationDate; set => warrantyExpirationDate = value; }
		public int Cost { get => cost; set => cost = value; }
		public string Notes { get => notes; set => notes = value; }
		public int Mispar_mosah { get => mispar_mosah; set => mispar_mosah = value; }
		public int CareID { get => careID; set => careID = value; }
		public string Useremail { get => useremail; set => useremail = value; }
		public int LicensePlate { get => licensePlate; set => licensePlate = value; }
		public string InvoiceFileName { get => invoiceFileName; set => invoiceFileName = value; }
		public string Carename { get => carename; set => carename = value; }
		public string GarageName { get => garagename; set => garagename = value; }

		public Log_Record() { }

		public Log_Record(int logID, int currentKM, DateTime recordDate, DateTime warrantyExpirationDate, int cost, string notes, int mispar_mosah, int careID, string useremail, int licensePlate, string invoiceFileName, string carename,string garagename)
		{
			this.LogID = logID;
			this.CurrentKM = currentKM;
			this.RecordDate = recordDate;
			this.WarrantyExpirationDate = warrantyExpirationDate;
			this.Cost = cost;
			this.Notes = notes;
			this.Mispar_mosah = mispar_mosah;
			this.CareID = careID;
			this.Useremail = useremail;
			this.LicensePlate = licensePlate;
			this.InvoiceFileName = invoiceFileName;
			this.Carename = carename;
			this.GarageName = garagename;


		}

		public int InsertLog_Record()
        {
            DBservices dbs = new DBservices();

            return dbs.InsertLog_Record(this);
        }


		public int DeleteLog()
		{
			DBservices dbs = new DBservices();
			return dbs.DeleteLogRecord(this.LogID);
		}

		public List<Log_Record> GetCarUserLog(string email)
		{
			DBservices dbs = new DBservices();
			return dbs.GetCarUserLog(email);
		}

		public List<Log_Record> ReadLogRecord(int LogID)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadLogRecord(LogID);
		}

		public int UpdateLog()
		{
			DBservices dbs = new DBservices();
			return dbs.UpdateLog(this);

		}

	}
}
