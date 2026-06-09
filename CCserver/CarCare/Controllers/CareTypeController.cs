using Microsoft.AspNetCore.Mvc;
using server.BL;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CareTypeController : ControllerBase
    {
        // GET: api/<CarTypeController>
        [HttpGet]
        public List<CareType> Get()
        {
			CareType types = new CareType();
		
			return types.ReadAllCareTypes();

		}

		[HttpGet("{id}")]
		public IEnumerable<CareType> Get(int id)
		{
            
			CareType TypeName = new CareType();
			return TypeName.ReadCareName(id);
		}


        // POST api/<CarTypeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<CarTypeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<CarTypeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
