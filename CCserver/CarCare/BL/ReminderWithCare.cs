using CarCare.BL;

namespace server.BL
{
	public class ReminderWithCare
	{

		public int ReminderID { get; set; }
		public DateTime RemindDate { get; set; }
		public string Notes { get; set; }
		public int CareID { get; set; }
		public string Email { get; set; }
		public int LicensePlate { get; set; }
		public string CareName { get; set; }
		public string CarNickName { get; set; }

	}


}
