using System.Reflection;

namespace server.BL
{
	public class VehicleLicenseData
	{
		string owner;
		int id;
		DateTime DateOfOwner;
		int PreviousOwners;

		public VehicleLicenseData(string owner, int id, DateTime dateOfOwner, int previousOwners)
		{
			Owner = owner;
			Id = id;
			DateOfOwner1 = dateOfOwner;
			PreviousOwners1 = previousOwners;
		}

		public string Owner { get => owner; set => owner = value; }
		public int Id { get => id; set => id = value; }
		public DateTime DateOfOwner1 { get => DateOfOwner; set => DateOfOwner = value; }
		public int PreviousOwners1 { get => PreviousOwners; set => PreviousOwners = value; }

		public List<CarModel> ReadCarModel(int licensePlate)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadCarModel(licensePlate);
		}

	}
}
