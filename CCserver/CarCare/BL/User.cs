using Microsoft.AspNetCore.Mvc;
using server;

namespace server.BL
{
	public class User
	{
		string firstName;
		string familyName;
		string password;
		string email;
		string phoneNum;
		bool verified;



        static List<User> userlist = new List<User>();
		public string FirstName { get => firstName; set => firstName = value; }
		public string FamilyName { get => familyName; set => familyName = value; }
		public string Password { get => password; set => password = value; }
		public string Email { get => email; set => email = value; }
		public string PhoneNum { get => phoneNum; set => phoneNum = value; }
		public bool Verified { get => verified; set => verified = value; }


		private object dbs;


		public User() { }

		public User(string FirstName, string FamilyName, string Password, string Email, string phoneNum)
		{
			this.FirstName = FirstName;
			this.FamilyName = FamilyName;
			this.Password = Password;
			this.Email = Email;
			this.PhoneNum = PhoneNum;
			this.Verified = this.Verified;

		}


		public int Insert()
		{
			DBservices dbs = new DBservices();
		
			return dbs.Insert(this);
		}

		public User Login()
		{

			DBservices dBservices = new DBservices();
			return dBservices.LogInUser(this.email,this.password);
		}

		public List<User> ReadUser()
		{
			DBservices dbs = new DBservices();
			return dbs.ReadUser();
		}




		public int Update()
		{
			DBservices dbs = new DBservices();
			return dbs.Update(this);
				
		}

		public int Delete()
		{
			DBservices dbs = new DBservices();
			return dbs.DeleteUser(this.email);
		}


	}



	}

