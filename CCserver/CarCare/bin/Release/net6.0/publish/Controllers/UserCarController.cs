
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

        //// GET api/<UserCarController>/5
        //[HttpGet("{imgName,type}")]//אין באמת צורך להכניס ערך ל"סוג" זה רק כדי להבדיל מהגט הקודם
        //public string GetTextFromImg(string imgName,string type)
        //{
        //    UserCar userCar = new UserCar();
        //    return userCar.HeOCR(imgName);

        //}



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

        // DELETE api/<UserController>/5
        [HttpDelete("{email,licence_plate}")]
        public int Delete(string email, int licence_Plate, [FromBody] UserCar userCar)
        {
            userCar.UserEmail = email;
            userCar.LicensePlate = licence_Plate;
            return userCar.Delete();
        }
    }
}
