using System.Reflection;

namespace server.BL
{
    public class CarModel
    {
        int licensePlate;
        string manufacturer;
        string model;
        int yearOfManufacture;
        string subModelCode;
        static List<CarModel> CarModelList = new List<CarModel>();

        public CarModel() { }
        public CarModel(int licensePlate, string manufacturer, string countryOfmanufacture, string model, int yearOfManufacture, string subModelCode)
        {
            LicensePlate = licensePlate;
            Manufacturer = manufacturer;
            Model = model;
            YearOfManufacture = yearOfManufacture;
            SubModelCode = subModelCode;
        }

        public int LicensePlate { get => licensePlate; set => licensePlate = value; }
        public string Manufacturer { get => manufacturer; set => manufacturer = value; }
        public string Model { get => model; set => model = value; }
        public int YearOfManufacture { get => yearOfManufacture; set => yearOfManufacture = value; }
        public string SubModelCode { get => subModelCode; set => subModelCode = value; }



        public List<CarModel> ReadCarModel(int licensePlate)
        {
            DBservices dbs = new DBservices();
            return dbs.ReadCarModel(licensePlate);
        }

    }
}
