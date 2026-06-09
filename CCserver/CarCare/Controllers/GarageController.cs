using CarCare.BL;
using Microsoft.AspNetCore.Mvc;
using server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GarageController : ControllerBase
    {
		[HttpGet("{yishuv}")]
		public IEnumerable<Garage> Get(string yishuv)
		{
			Garage garage = new Garage();
            return garage.ReadGarage(yishuv);
		}

		[HttpGet]

		public List<string> GETCITY()
        {
			Garage garage = new Garage();
			return garage.readecity();

        }

		[HttpGet("ReadGarageName/{id}")]
		public List<Garage> Get(int garageID)
		{

			Garage GarageName = new Garage();
			return GarageName.ReadGarageName(garageID);
		}

		// POST api/<CarModelController>
		[HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CarModelController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CarModelController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
