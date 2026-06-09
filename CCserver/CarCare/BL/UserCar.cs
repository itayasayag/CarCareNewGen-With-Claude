using System.ComponentModel;
using System.Text;
using System.Xml;
using IronOcr;
using System;

namespace server.BL

{

    public class UserCar
    {
        int licensePlate;
        string nickName;
        int currentKM;
        string picURL;
        string userEmail;
        bool isVerified;

        public UserCar() { }

        public UserCar(int licensePlate, string nickName, int currentKM, string picURL, string UserEmail, bool isVerified)
        {
            this.LicensePlate = licensePlate;
            this.NickName = nickName;
            this.CurrentKM = currentKM;
            this.PicURL = picURL;
            this.UserEmail = userEmail;
            this.isVerified = isVerified;
        }

        public int LicensePlate { get => licensePlate; set => licensePlate = value; }
        public string NickName { get => nickName; set => nickName = value; }
        public int CurrentKM { get => currentKM; set => currentKM = value; }
        public string PicURL { get => picURL; set => picURL = value; }
        public string UserEmail { get => userEmail; set => userEmail = value; }
        public bool IsVerified {  get => isVerified; set => isVerified = value;  }



        public IEnumerable<UserCar> ReadUserCar()
        {
            DBservices dbs = new DBservices();
            return dbs.ReadUserCar();
        }


        public int Insert()
        {
            DBservices dbs = new DBservices();

            return dbs.InsertUserCar(this);
        }


        public int Update()
        {
            DBservices dbs = new DBservices();
            return dbs.UpdateUserCar(this);
        }


        public int DeActivateCar()
        {
            DBservices dbs = new DBservices();
            return dbs.DeActivateUserCar(this);
        }

        public int SetUserCarAsVerified(string email, int licence_Plate)
        {
            DBservices dbs = new DBservices();
            return dbs.SetUserCarAsVerified(email,licence_Plate);
        }

        internal IEnumerable<UserCar> ReadUserCarByEmail(string email)
        {
            DBservices dbs = new DBservices();
            return dbs.ReadUserCarByEmail(email);
        }

        public string ReadNickName(string email, int lplate)
        {
            DBservices dbs = new DBservices();
            return dbs.GetNickName(email, lplate);
        }

        public int SendDriver(string NewMail, string OldMail, int Lplate)
        {
            DBservices dbs = new DBservices();
            return dbs.SendDrivertoDB(NewMail, OldMail, Lplate);
        }
        public string HeOCR(string imgName)
        {

            var Ocr = new IronTesseract();
            Ocr.Language = OcrLanguage.Hebrew;
            using (var Input = new OcrInput(@"uploadedFiles\" + imgName + ".jpg"))
            {
                var Result = Ocr.Read(Input);
                string AllText = Result.Text;
                return AllText;
            }



        }


    }
}
