using CarCare.BL;

namespace server.BL
{
	public class CareType
	{
		int careID;
		string careName;
		int recDaysForRepeat;
		int recKMForRepeat;

		public CareType() { }
		public CareType(int careID, string careName, int recDaysForRepeat, int recKMForRepeat)
		{
			this.CareID = careID;
			this.CareName = careName;
			this.RecDaysForRepeat = recDaysForRepeat;
			this.RecKMForRepeat = recKMForRepeat;
		}

		public int CareID { get => careID; set => careID = value; }
		public string CareName { get => careName; set => careName = value; }
		public int RecDaysForRepeat { get => recDaysForRepeat; set => recDaysForRepeat = value; }
		public int RecKMForRepeat { get => recKMForRepeat; set => recKMForRepeat = value; }

		public List<CareType> ReadAllCareTypes()
		{
			DBservices dbs = new DBservices();
			return dbs.ReadAllCareTypes();
		}

		public List<CareType> ReadCareName(int CareID)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadCareName(CareID);
		}

	}


}



