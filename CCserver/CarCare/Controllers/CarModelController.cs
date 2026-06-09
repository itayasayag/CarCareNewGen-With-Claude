using Microsoft.AspNetCore.Mvc;
using server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarModelController : ControllerBase
    {
		[HttpGet("{licensePlate}")]
		public IEnumerable<CarModel> Get(int licensePlate)
		{
			CarModel Model = new CarModel();
			return Model.ReadCarModel(licensePlate);
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
