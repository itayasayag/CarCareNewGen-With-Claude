using server.BL;

namespace CarCare.BL
{
    public class Garage
    {

        int id;
        string shem_mosah;
        string sug_mosah;
        string ktovet;
        string yishuv;
        string telephone;
        int rate;

        public Garage() { }

        public Garage(int id, string shem_mosah, string sug_mosah, string ktovet, string yishuv, string telephone, int rate)
        {
            this.Id = id;
            this.Shem_mosah = shem_mosah;
            this.Sug_mosah = sug_mosah;
            this.Ktovet = ktovet;
            this.Yishuv = yishuv;
            this.Telephone = telephone;
            this.rate = rate;
        }
        //שינוי לבדיקה

        public int Id { get => id; set => id = value; }
        public string Shem_mosah { get => shem_mosah; set => shem_mosah = value; }
        public string Sug_mosah { get => sug_mosah; set => sug_mosah = value; }
        public string Ktovet { get => ktovet; set => ktovet = value; }
        public string Yishuv { get => yishuv; set => yishuv = value; }
        public string Telephone { get => telephone; set => telephone = value; }
        public int Rate { get => rate; set => rate = value; }

		public List<Garage> ReadGarage(string yishuv)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadGarage(yishuv);
		}

        public List<string> readecity()
        {
            DBservices dbs = new DBservices();
            return dbs.ReadCity();
        }
		public List<Garage> ReadGarageName(int GarageID)
		{
			DBservices dbs = new DBservices();
			return dbs.ReadGarageName(GarageID);
		}



	}




}
