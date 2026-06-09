
using server.BL;
using Microsoft.AspNetCore.Mvc;



namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserCarController : ControllerBase
    {
        // GET: api/<UserCarController>
        [HttpGet]
        public IEnumerable<UserCar> Get()
        {
            UserCar userCar = new UserCar();
            return userCar.ReadUserCar();
        }

        //GET api/<UserCarController>/5
        [HttpGet("{email}")]
        public IEnumerable<UserCar> GetByEmail(string email)
        {
            UserCar userCar = new UserCar();
            return userCar.ReadUserCarByEmail(email);

        }

        [HttpGet("getNickname/{email,Lplate}")]
        public string GetTheNickname(string email, int lplate)
        {
            UserCar userCar = new UserCar();
            return userCar.ReadNickName(email,lplate);

        }



        // POST api/<UserCarController>
        [HttpPost]
        public int Post([FromBody] UserCar userCar)
        {
            return userCar.Insert();
        }



		// PUT api/<UserController>/5
		[HttpPut("{email,licence_plate}")]
        public int Put(string email,int licence_Plate, [FromBody] UserCar userCar)
        {
            userCar.UserEmail = email;
            userCar.LicensePlate = licence_Plate;
            return userCar.Update();

        }

        // PUT api/<UserController>/6 TBD
        [HttpPut]
        public int PutDeActivate(string email, int licence_Plate, [FromBody] UserCar userCar)
        {
            return userCar.DeActivateCar();

        }

        // PUT api/<UserController>/7 
        [HttpPut("SendCar/")]
        public int SendNewDriver(string NewEmail, string OldMail, int Lplate)
        {
            UserCar userCar = new UserCar();
            return userCar.SendDriver(NewEmail, OldMail, Lplate);

        }
        [HttpPut("SetVerified/")]
        public int SetUserCarAsVerified(string email, int licence_Plate)
        {
            UserCar userCar = new UserCar();
            return userCar.SetUserCarAsVerified(email,licence_Plate);

        }

    }
}
