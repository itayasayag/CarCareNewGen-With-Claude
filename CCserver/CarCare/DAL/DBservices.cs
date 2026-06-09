using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data;
using System.Text;
using server.BL;
using server.Controllers;
using System.Globalization;
using System.Collections;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Reflection;
using CarCare.BL;
using System.Numerics;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using System.Drawing;

/// <summary>
/// DBServices is a class created by me to provides some DataBase Services
/// </summary>
public class DBservices
{
	private readonly int licensePlate;

	public DBservices()
	{
		//
		// TODO: Add constructor logic here
		//
	}

	//--------------------------------------------------------------------------------------------------
	// This method creates a connection to the database according to the connectionString name in the web.config 
	//--------------------------------------------------------------------------------------------------
	public SqlConnection connect(String conString)
	{

		// read the connection string from the configuration file
		IConfigurationRoot configuration = new ConfigurationBuilder()
		.AddJsonFile("appsettings.json").Build();
		string cStr = configuration.GetConnectionString("myProjDB");
		SqlConnection con = new SqlConnection(cStr);
		con.Open();
		return con;
	}

	//*********************************************************************** start U S E R ********************************************************//


	//INSERT USER
	public int Insert(User user)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = CreateInsertWithStoredProcedure("CreateUser", con, user);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand CreateInsertWithStoredProcedure(String spName, SqlConnection con, User user)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@firstName", user.FirstName);
		cmd.Parameters.AddWithValue("@familyName", user.FamilyName);
		cmd.Parameters.AddWithValue("@password", user.Password);
		cmd.Parameters.AddWithValue("@email", user.Email);
		cmd.Parameters.AddWithValue("@phoneNum", user.PhoneNum);
		cmd.Parameters.AddWithValue("@verified", false);

		return cmd;
	}




	//READ USER
	public List<User> ReadUser()
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<User> users = new List<User>();

		cmd = buildReadStoredProcedureCommand(con, "ReadAllUsers");

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			User u = new User();
			u.FirstName = dataReader["FirstName"].ToString();
			u.FamilyName = dataReader["FamilyName"].ToString();
			u.Password = dataReader["Password"].ToString();
			u.Email = dataReader["Email"].ToString();
			u.PhoneNum = dataReader["PhoneNum"].ToString();

			users.Add(u);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return users;


	}
	SqlCommand buildReadStoredProcedureCommand(SqlConnection con, string spName)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		return cmd;

	}




	//Log-In USER (return User By Email&Password)
	public User LogInUser(string email, string password)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = getLogedUserByStoredProcedure("GetUserByEmailAndPassword", con, email, password);

		User u = new User();
		try
		{
			SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			if (dataReader.Read())
			{

				u.FirstName = dataReader["firstName"].ToString();
				u.FamilyName = dataReader["familyName"].ToString();
				u.Password = dataReader["password"].ToString();
				u.Email = dataReader["email"].ToString();
				u.PhoneNum = dataReader["phoneNum"].ToString();


			}
			else
			{
				// No matching user found, throw an exception or handle accordingly
				throw new InvalidOperationException("Invalid email or password");
			}

		}
		catch (Exception ex)
		{
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

		return u;

	}
	private SqlCommand getLogedUserByStoredProcedure(String spName, SqlConnection con, string email, string password)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@Email", email);
		cmd.Parameters.AddWithValue("@Password", password);

		return cmd;
	}




	//Update USER
	public int Update(User user)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = UpdateCommandWithStoredProcedure("UpdateUser", con, user);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand UpdateCommandWithStoredProcedure(String email, SqlConnection con, User user)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = email;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@firstName", user.FirstName);
		cmd.Parameters.AddWithValue("@familyName", user.FamilyName);
		cmd.Parameters.AddWithValue("@password", user.Password);
		cmd.Parameters.AddWithValue("@email", user.Email);
		cmd.Parameters.AddWithValue("@phoneNum", user.PhoneNum);



		return cmd;
	}




	//מחיקת טיפול
	public int DeleteLogRecord(int logID)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = DeleteLogRecordWithStoredProcedure("DeleteLog_Record", con, logID);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}
	private SqlCommand DeleteLogRecordWithStoredProcedure(String spName, SqlConnection con, int logID)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@logID", logID);

		return cmd;
	}


	//*********************************************************************** end U S E R ********************************************************//




	//*********************************************************************** start USER CAR ********************************************************//

	//READ USER CAR
	public List<UserCar> ReadUserCar()
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<UserCar> usersCars = new List<UserCar>();

		cmd = ReadUserCarStoredProcedureCommand(con, "ReadAllUserCars");

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{

			UserCar US = new UserCar();
			US.LicensePlate = (int)dataReader["licensePlate"];
			US.UserEmail = dataReader["email"].ToString();
			US.NickName = dataReader["nickName"].ToString();
			US.CurrentKM = (int)dataReader["currentKM"];
			US.PicURL = dataReader["picURL"].ToString();


			usersCars.Add(US);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return usersCars;


	}
	SqlCommand ReadUserCarStoredProcedureCommand(SqlConnection con, string spName)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		return cmd;

	}

	//READ USER CAR BY EMAIL
	public List<UserCar> ReadUserCarByEmail(string email)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<UserCar> usersCars = new List<UserCar>();

		cmd = ReadUserCarByEmailStoredProcedureCommand(con, "ReadUserCarsByEmail", email);

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{

			UserCar US = new UserCar();
			US.LicensePlate = (int)dataReader["licensePlate"];
			US.UserEmail = dataReader["email"].ToString();
			US.NickName = dataReader["nickName"].ToString();
			US.CurrentKM = (int)dataReader["currentKM"];
			US.PicURL = dataReader["picURL"].ToString();
			US.IsVerified = dataReader["IsVerified"] != DBNull.Value && (bool)dataReader["IsVerified"];




			usersCars.Add(US);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return usersCars;


	}
	SqlCommand ReadUserCarByEmailStoredProcedureCommand(SqlConnection con, string spName, string email)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@Email", email);

		return cmd;

	}






	//INSERT USER CAR
	public int InsertUserCar(UserCar userCar)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log TBD תוקע את המערכת
			throw (ex);
		}

		cmd = CreateUserCarInsertWithStoredProcedure("CreateUserCar", con, userCar);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log TBD תוקעs את המערכת
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	SqlCommand CreateUserCarInsertWithStoredProcedure(String spName, SqlConnection con, UserCar userCar)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@licensePlate", userCar.LicensePlate);
		cmd.Parameters.AddWithValue("@nickName", userCar.NickName);
		cmd.Parameters.AddWithValue("@currentKM", userCar.CurrentKM);
		cmd.Parameters.AddWithValue("@picURL", userCar.PicURL);
		cmd.Parameters.AddWithValue("@email", userCar.UserEmail);

		return cmd;
	}




	//Update USER CAR
	public int UpdateUserCar(UserCar userCar)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = UpdateUserCarCommandWithStoredProcedure("UpdateUserCar", con, userCar);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand UpdateUserCarCommandWithStoredProcedure(String email, SqlConnection con, UserCar userCar)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = email;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@licensePlate", userCar.LicensePlate);
		cmd.Parameters.AddWithValue("@nickName", userCar.NickName);
		cmd.Parameters.AddWithValue("@currentKM", userCar.CurrentKM);
		cmd.Parameters.AddWithValue("@picURL", userCar.PicURL);
		cmd.Parameters.AddWithValue("@email", userCar.UserEmail);



		return cmd;
	}

	//set USER CAR is verified IsVrified=1
	public int SetUserCarAsVerified(string email, int licence_Plate)
	{

		SqlConnection con;
		SqlCommand cmd;
		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = SetUserCarAsVerifiedSP("SetAsVerified", con, email, licence_Plate);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}
	private SqlCommand SetUserCarAsVerifiedSP(String spName, SqlConnection con, string email, int lPlate)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@UserEmail", email);
		cmd.Parameters.AddWithValue("@LP", lPlate);

		return cmd;
	}


	//DeActivate USER CAR IsActive=0
	public int DeActivateUserCar(UserCar userCar)
	{

		SqlConnection con;
		SqlCommand cmd;
		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = DeActivateUserCarWithSP("DeActivatedUserCar", con, userCar.UserEmail, userCar.LicensePlate);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}
	private SqlCommand DeActivateUserCarWithSP(String spName, SqlConnection con, string email, int lPlate)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@licensePlate", lPlate);
		cmd.Parameters.AddWithValue("@email", email);

		return cmd;
	}

	public int SendDrivertoDB(string NewUser, string OldUser, int Lplate)
	{

		SqlConnection con;
		SqlCommand cmd;
		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = SendCarWithSP("SendCar", con, NewUser, OldUser, Lplate);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}

	private SqlCommand SendCarWithSP(String spName, SqlConnection con, string NewEmail, string OldMail, int LicensePlate)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@NewUserEmail", NewEmail);
		cmd.Parameters.AddWithValue("@OldEmail", OldMail);
		cmd.Parameters.AddWithValue("@licensePlate", LicensePlate);

		return cmd;
	}

	public string GetNickName(string email, int lplate)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = getNickNameByEmailLplate("[ReadUserCarNickName]", con, email, lplate);

		string US = null;
		try
		{
			SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			while (dataReader.Read())
			{

				US = dataReader["nickName"].ToString();
			}

		}
		catch (Exception ex)
		{
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

		return US;

	}
	private SqlCommand getNickNameByEmailLplate(String spName, SqlConnection con, string email, int licenseplate)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@email", email);
		cmd.Parameters.AddWithValue("@licensePlate", licenseplate);


		return cmd;
	}

	//*********************************************************************** end USER CAR ********************************************************//



	//*********************************************************************** start CAR Model ********************************************************//


	//READ carModel

	public List<CarModel> ReadCarModel(int licensePlate)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<CarModel> models = new List<CarModel>();

		cmd = buildReadCarModelStoredProcedureCommand(con, "ReadCarGov_il", licensePlate);

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			CarModel c = new CarModel();

			c.LicensePlate = (int)dataReader["mispar_rechev"];
			c.Manufacturer = dataReader["tozeret_nm"].ToString();
			c.SubModelCode = dataReader["ramat_gimur"].ToString();
			c.YearOfManufacture = (int)(short)dataReader["shnat_yitzur"];
			c.Model = dataReader["kinuy_mishari"].ToString();

			models.Add(c);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return models;


	}
	SqlCommand buildReadCarModelStoredProcedureCommand(SqlConnection con, string spName, int licensePlate)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@licensePlate", licensePlate);



		return cmd;

	}



	//*********************************************************************** end CAR Model ********************************************************//

	//*********************************************************************** start log record ********************************************************//

	//הוספת טיפול

	public int InsertLog_Record(Log_Record LogRecord)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = CreateLogRecordWithStoredProcedure("CreateLog_Record", con, LogRecord);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand CreateLogRecordWithStoredProcedure(String spName, SqlConnection con, Log_Record LogRecord)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@recordDate", LogRecord.RecordDate);
		cmd.Parameters.AddWithValue("@currentKM", LogRecord.CurrentKM);
		cmd.Parameters.AddWithValue("@cost", LogRecord.Cost);
		cmd.Parameters.AddWithValue("@notes", LogRecord.Notes);
		cmd.Parameters.AddWithValue("@email", LogRecord.Useremail);
		cmd.Parameters.AddWithValue("@warrantyExpirationDate", LogRecord.WarrantyExpirationDate);
		cmd.Parameters.AddWithValue("@mispar_mosah", LogRecord.Mispar_mosah);
		cmd.Parameters.AddWithValue("@licensePlate", LogRecord.LicensePlate);
		cmd.Parameters.AddWithValue("@careID", LogRecord.CareID);
		cmd.Parameters.AddWithValue("@invoiceFileName", LogRecord.InvoiceFileName);


		return cmd;
	}

	public int DeleteUser(string email)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = DeleteWithStoredProcedure("DeleteUser", con, email);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}

	//מחיקת טיפול
	private SqlCommand DeleteWithStoredProcedure(String spName, SqlConnection con, string email)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@email", email);

		return cmd;
	}

	//קריאת טיפול עבור רכב של משתמש
	public List<Log_Record> GetCarUserLog(string email)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = getCarUserLogByStoredProcedure("ReadLog_RecordByEmail", con, email);

		List<Log_Record> logs = new List<Log_Record>();
		try
		{
			SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			while (dataReader.Read())
			{
				Log_Record LR = new Log_Record();


				LR.LogID = (int)dataReader["logID"];
				LR.CurrentKM = (int)dataReader["currentKM"];
				LR.RecordDate = (DateTime)dataReader["recordDate"];
				LR.WarrantyExpirationDate = (DateTime)dataReader["warrantyExpirationDate"];
				LR.Cost = (int)dataReader["cost"];
				LR.Notes = dataReader["notes"].ToString();
				LR.Mispar_mosah = (int)dataReader["mispar_mosah"];
				LR.CareID = (int)dataReader["careID"];
				LR.Useremail = dataReader["email"].ToString();
				LR.LicensePlate = (int)dataReader["licensePlate"];
				LR.InvoiceFileName = dataReader["invoiceFileName"].ToString();

				CareType care = new CareType();
				List<CareType> CareName = care.ReadCareName(LR.CareID);
				LR.Carename = CareName[0].CareName.ToString();

				Garage garage = new Garage();
				List<Garage> garageName = garage.ReadGarageName(LR.Mispar_mosah);
				LR.GarageName = garageName[0].Shem_mosah.ToString();


				logs.Add(LR);

			}


		}
		catch (Exception ex)
		{
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

		return logs;

	}
	private SqlCommand getCarUserLogByStoredProcedure(String spName, SqlConnection con, string email)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@Email", email);


		return cmd;
	}

//קריאת טיפול יחיד ע"י logid
	public List<Log_Record> ReadLogRecord(int LogID)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = GETlogrecordByLogID("ReadLog_RecordByLogID", con, LogID);

		List<Log_Record> logs = new List<Log_Record>();
		try
		{
			SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
			while (dataReader.Read())
			{
				Log_Record LR = new Log_Record();


				LR.LogID = (int)dataReader["logID"];
				LR.CurrentKM = (int)dataReader["currentKM"];
				LR.RecordDate = (DateTime)dataReader["recordDate"];
				LR.WarrantyExpirationDate = (DateTime)dataReader["warrantyExpirationDate"];
				LR.Cost = (int)dataReader["cost"];
				LR.Notes = dataReader["notes"].ToString();
				LR.Mispar_mosah = (int)dataReader["mispar_mosah"];
				LR.CareID = (int)dataReader["careID"];
				LR.Useremail = dataReader["email"].ToString();
				LR.LicensePlate = (int)dataReader["licensePlate"];
				LR.InvoiceFileName = dataReader["invoiceFileName"].ToString();

				CareType care = new CareType();
				List<CareType> CareName = care.ReadCareName(LR.CareID);
				LR.Carename = CareName[0].CareName.ToString();
				Garage garage = new Garage();
				List<Garage> GarageName = garage.ReadGarageName(LR.Mispar_mosah);
				LR.GarageName = GarageName[0].Shem_mosah.ToString();

				logs.Add(LR);

			}


		}
		catch (Exception ex)
		{
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

		return logs;

	}
	private SqlCommand GETlogrecordByLogID(String spName, SqlConnection con, int LogID)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@logID", LogID);


		return cmd;
	}

	//עדכון טיפול
	public int UpdateLog(Log_Record logRecord)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = UpdateLogRecordCommandWithStoredProcedure("UpdateLogRecord", con, logRecord);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand UpdateLogRecordCommandWithStoredProcedure(String spName, SqlConnection con, Log_Record logRecord)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text


		cmd.Parameters.AddWithValue("@logID", logRecord.LogID);
		cmd.Parameters.AddWithValue("@currentKM", logRecord.CurrentKM);
		cmd.Parameters.AddWithValue("@careID", logRecord.CareID);
		cmd.Parameters.AddWithValue("@notes", logRecord.Notes);
		cmd.Parameters.AddWithValue("@cost", logRecord.Cost);
		cmd.Parameters.AddWithValue("@invoiceFileName", logRecord.InvoiceFileName);




		return cmd;
	}
	//*********************************************************************** end log record ********************************************************//


	//*********************************************************************** start Garage ********************************************************//





	public List<Garage> ReadGarage(string yishuv)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<Garage> garage = new List<Garage>();

		cmd = buildReadGarageStoredProcedureCommand(con, "[ReadgarageGovILByYishuv]", yishuv);

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			Garage g = new Garage();

			g.Id = (int)dataReader["mispar_mosah"];
			g.Shem_mosah = dataReader["shem_mosah"].ToString();
			g.Sug_mosah = dataReader["sug_mosah"].ToString();
			g.Ktovet = dataReader["ktovet"].ToString();
			g.Yishuv = dataReader["Yishuv"].ToString();
			g.Telephone = dataReader["Telephone"].ToString();



			garage.Add(g);


		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return garage;


	}
	SqlCommand buildReadGarageStoredProcedureCommand(SqlConnection con, string spName, string yishuv)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@yishuv", yishuv);

		return cmd;

	}

	//קריאה של ישובים מתוך טבלת המוסכים

	public List<string> ReadCity()
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<string> cities = new List<string>();

		cmd = buildReadGaragecitiesStoredProcedureCommand(con, "ReadCitiesFromGarage");

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			Garage g = new Garage();


			g.Yishuv = dataReader["Yishuv"].ToString();

			cities.Add(g.Yishuv);


		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return cities;


	}
	SqlCommand buildReadGaragecitiesStoredProcedureCommand(SqlConnection con, string spName)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text



		return cmd;

	}

	public List<Garage> ReadGarageName(int GarageID)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<Garage> garagename = new List<Garage>();

		cmd = ReadGarageNameFromSP(con, "ReadgarageNameGovILByID", GarageID);


		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			Garage g = new Garage();

			g.Shem_mosah = dataReader["shem_mosah"].ToString();


			garagename.Add(g);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return garagename;


	}
	SqlCommand ReadGarageNameFromSP(SqlConnection con, string spName, int GarageID)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@GarageID", GarageID);



		return cmd;

	}

	//*********************************************************************** end Garage ********************************************************//

	//*********************************************************************** START CareType ********************************************************//

	public List<CareType> ReadAllCareTypes()
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<CareType> caretypes = new List<CareType>();

		cmd = ReadAllCareTypesStoredProcedureCommand(con, "ReadAllCareType");

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{

			CareType CT = new CareType();


			CT.CareID = (int)dataReader["careID"];
			CT.CareName = dataReader["CareName"].ToString();
			CT.RecDaysForRepeat = (int)dataReader["recDaysForRepeat"];
			CT.RecKMForRepeat = (int)(long)dataReader["recKMForRepeat"];


			caretypes.Add(CT);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return caretypes;


	}
	SqlCommand ReadAllCareTypesStoredProcedureCommand(SqlConnection con, string spName)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		return cmd;

	}

	public List<CareType> ReadCareName(int CareID)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		List<CareType> CareName = new List<CareType>();

		cmd = buildReadCareNameStoredProcedureCommand(con, "ReadCareType", CareID);


		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{
			CareType c = new CareType();

			c.CareName = dataReader["careName"].ToString();


			CareName.Add(c);
		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return CareName;


	}
	SqlCommand buildReadCareNameStoredProcedureCommand(SqlConnection con, string spName, int CareID)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@careID", CareID);



		return cmd;

	}





	//*********************************************************************** END CareType ********************************************************//


	//*********************************************************************** start reminder ********************************************************//


	//read all user reminder (by user email)
	public List<ReminderWithCare> ReadReminderByEmail(string email)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}
		List<ReminderWithCare> reminders = new List<ReminderWithCare>();


		cmd = buildReadReminderByUserStoredProcedureCommand(con, "ReadReminderByUser", email);

		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

		while (dataReader.Read())
		{

			ReminderWithCare RWC = new ReminderWithCare();




			RWC.ReminderID = (int)dataReader["reminderID"];
			RWC.RemindDate = (DateTime)dataReader["remindDate"];
			RWC.Notes = dataReader["notes"].ToString();
			RWC.CareID = (int)dataReader["CareID"];
			RWC.Email = dataReader["Email"].ToString();
			RWC.LicensePlate = (int)dataReader["LicensePlate"];
			RWC.CarNickName = dataReader["nickname"].ToString();


			CareType care = new CareType();
			List<CareType> CareName = care.ReadCareName(RWC.CareID);
			RWC.CareName = CareName[0].CareName.ToString();
			Garage garage = new Garage();


			reminders.Add(RWC);


		}

		if (con != null)
		{
			// close the db connection
			con.Close();
		}

		return reminders;


	}
	SqlCommand buildReadReminderByUserStoredProcedureCommand(SqlConnection con, string spName, string email)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@email", email);

		return cmd;

	}


	//INSERT REMINDER
	public int InserReminder(Reminder reminder)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{

			throw (ex);
		}

		cmd = CreateReminderInsertWithStoredProcedure("CreateReminder", con, reminder);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log TBD תוקעs את המערכת
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	SqlCommand CreateReminderInsertWithStoredProcedure(String spName, SqlConnection con, Reminder reminder)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@remindDate", reminder.RemindDate);
		cmd.Parameters.AddWithValue("@notes", reminder.Notes);
		cmd.Parameters.AddWithValue("@email", reminder.Email);
		cmd.Parameters.AddWithValue("@careID", reminder.CareID);
		cmd.Parameters.AddWithValue("@licensePlate", reminder.LicensePlate);


		return cmd;
	}

	//Update REMINSER
	public int UpdateReminder(Reminder reminder)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = UpdateReminderCommandWithStoredProcedure("UpdateReminder", con, reminder);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}

	}
	private SqlCommand UpdateReminderCommandWithStoredProcedure(String spName, SqlConnection con, Reminder reminder)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@reminderID", reminder.ReminderID);
		cmd.Parameters.AddWithValue("@remindDate", reminder.RemindDate);
		cmd.Parameters.AddWithValue("@notes", reminder.Notes);



		return cmd;
	}


	public int DeleteReminder(int id)
	{

		SqlConnection con;
		SqlCommand cmd;

		try
		{
			con = connect("myProjDB"); // create the connection
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		cmd = DeleteReminderWithStoredProcedure("DeleteReminder", con, id);             // create the command

		try
		{
			int numEffected = cmd.ExecuteNonQuery(); // execute the command
			return numEffected;
		}
		catch (Exception ex)
		{
			// write to log
			throw (ex);
		}

		finally
		{
			if (con != null)
			{
				// close the db connection
				con.Close();
			}
		}
	}
	private SqlCommand DeleteReminderWithStoredProcedure(String spName, SqlConnection con, int id)
	{

		SqlCommand cmd = new SqlCommand(); // create the command object

		cmd.Connection = con;              // assign the connection to the command object

		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

		cmd.Parameters.AddWithValue("@Reminderid", id);

		return cmd;
	}
}







	//*********************************************************************** END reminder ********************************************************//







//	//*****************************start F L A T ****************************/
//	public int InsertFlat(Flat flat)
//    {

//        SqlConnection con;
//        SqlCommand cmd;

//        try
//        {
//            con = connect("myProjDB"); // create the connection
//        }
//        catch (Exception ex)
//        {
//            // write to log
//            throw (ex);
//        }

//        cmd = CreateInsertFlatWithStoredProcedure("CreateFlat", con, flat);             // create the command

//        try
//        {
//            int numEffected = cmd.ExecuteNonQuery(); // execute the command
//            return numEffected;
//        }
//        catch (Exception ex)
//        {
//            // write to log
//            throw (ex);
//        }

//        finally
//        {
//            if (con != null)
//            {
//                // close the db connection
//                con.Close();
//            }
//        }

//    }

//    private SqlCommand CreateInsertFlatWithStoredProcedure(String spName, SqlConnection con, Flat flat)
//    {

//        SqlCommand cmd = new SqlCommand(); // create the command object

//        cmd.Connection = con;              // assign the connection to the command object

//        cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

//        cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

//        cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

//        cmd.Parameters.AddWithValue("@city", flat.City);

//        cmd.Parameters.AddWithValue("@address", flat.Address);
//        cmd.Parameters.AddWithValue("@price", flat.Price);
//        cmd.Parameters.AddWithValue("@numOfRooms", flat.NumOfRooms);

//        return cmd;
//    }

//	public List<Flat> ReadFlat()
//	{

//		SqlConnection con;
//		SqlCommand cmd;

//		try
//		{
//			con = connect("myProjDB"); // create the connection
//		}
//		catch (Exception ex)
//		{
//			// write to log
//			throw (ex);
//		}

//		List<Flat> flat = new List<Flat>();

//		cmd = buildReadFlatStoredProcedureCommand(con, "ReadAllFlats");

//		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

//		while (dataReader.Read())
//		{
//			Flat f = new Flat();
//            f.Id = (int)dataReader["Id"];
//            f.City = dataReader["City"].ToString();
//            f.Address= dataReader["Address"].ToString();
//			f.Price = (double)(decimal)dataReader["Price"];
//            f.NumOfRooms= (int)dataReader["NumOfRooms"];

//			flat.Add(f);
//		}

//		if (con != null)
//		{
//			// close the db connection
//			con.Close();
//		}

//		return flat;


//	}

//	SqlCommand buildReadFlatStoredProcedureCommand(SqlConnection con, string spName)
//	{

//		SqlCommand cmd = new SqlCommand(); // create the command object

//		cmd.Connection = con;              // assign the connection to the command object

//		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

//		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

//		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

//		return cmd;

//	}

//	//*****************************end F L A T *****************************//

//	//*****************************start VACATION *****************************//


//	public List<Vacation> ReadVacation()
//	{

//		SqlConnection con;
//		SqlCommand cmd;

//		try
//		{
//			con = connect("myProjDB"); // create the connection
//		}
//		catch (Exception ex)
//		{
//			// write to log
//			throw (ex);
//		}

//		List<Vacation> vacation = new List<Vacation>();

//		cmd = buildReadVacationStoredProcedureCommand(con, "ReadAllVacations");

//		SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

//		while (dataReader.Read())
//		{

//			Vacation v = new Vacation();
//			v.Id = (int)dataReader["Id"];
//			v.UserId = dataReader["usereMail"].ToString();
//			v.FlatId = (int)dataReader["FlatId"];
//			v.StartDate = (DateTime)dataReader["StartDate"];
//			v.EndDate = (DateTime)dataReader["EndDate"];

//			vacation.Add(v);
//		}

//		if (con != null)
//		{
//			// close the db connection
//			con.Close();
//		}

//		return vacation;


//	}

//	SqlCommand buildReadVacationStoredProcedureCommand(SqlConnection con, string spName)
//	{

//		SqlCommand cmd = new SqlCommand(); // create the command object

//		cmd.Connection = con;              // assign the connection to the command object

//		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

//		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

//		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

//		return cmd;

//	}
//	public int InsertVacation(Vacation vacation)
//	{

//		SqlConnection con;
//		SqlCommand cmd;

//		try
//		{
//			con = connect("myProjDB"); // create the connection
//		}
//		catch (Exception ex)
//		{
//			// write to log
//			throw (ex);
//		}

//		cmd = CreateInserVacationtWithStoredProcedure("CreateVacation", con, vacation);             // create the command

//		try
//		{
//			int numEffected = cmd.ExecuteNonQuery(); // execute the command
//			return numEffected;
//		}
//		catch (Exception ex)
//		{
//			// write to log
//			throw (ex);
//		}

//		finally
//		{
//			if (con != null)
//			{
//				// close the db connection
//				con.Close();
//			}
//		}

//	}



//	private SqlCommand CreateInserVacationtWithStoredProcedure(String spName, SqlConnection con, Vacation vacation)
//	{

//		SqlCommand cmd = new SqlCommand(); // create the command object

//		cmd.Connection = con;              // assign the connection to the command object

//		cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

//		cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

//		cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text


//		cmd.Parameters.AddWithValue("@flatId", vacation.FlatId);
//		cmd.Parameters.AddWithValue("@usereMail", vacation.UserId);
//		cmd.Parameters.AddWithValue("@startDate", vacation.StartDate);
//		cmd.Parameters.AddWithValue("@endDate", vacation.EndDate);

//		return cmd;
//	}

//    //*****************************end VACATION *****************************//


//    //*****************************start adHoc *****************************//

//    public Object getAvgPrice(int monthnum)
//    {

//        SqlConnection con;
//        SqlCommand cmd;

//        try
//        {
//            con = connect("myProjDB"); // create the connection
//        }
//        catch (Exception ex)
//        {
//            // write to log
//            throw (ex);
//        }


//		List<Object> OList = new List<Object>();

//		cmd = getAvgPriceOfCityByMonthWithSP(con, "GetAveragePricePerNight", monthnum);

//        SqlDataReader dataReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

//        while (dataReader.Read())
//        {
//            var jsonObject = new
//            {
//                month = dataReader["month"].ToString(),
//                city = dataReader["city"].ToString(),
//                average_price_per_night = dataReader["average_price_per_night"].ToString()
//            };

//            OList.Add(jsonObject);
//        }

//        if (con != null)
//        {
//            // close the db connection
//            con.Close();
//        }

//        return OList;


//    }

//    SqlCommand getAvgPriceOfCityByMonthWithSP(SqlConnection con, string spName, int month)
//    {

//        SqlCommand cmd = new SqlCommand(); // create the command object

//        cmd.Connection = con;              // assign the connection to the command object

//        cmd.CommandText = spName;      // can be Select, Insert, Update, Delete 

//        cmd.CommandTimeout = 10;           // Time to wait for the execution' The default is 30 seconds

//        cmd.CommandType = System.Data.CommandType.StoredProcedure; // the type of the command, can also be text

//        cmd.Parameters.AddWithValue("@selectedMonth", month);

//        return cmd;

//    }


//    //*****************************end adHoc *****************************//
//}
