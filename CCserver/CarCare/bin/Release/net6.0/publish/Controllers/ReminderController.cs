using CarCare.BL;
using Microsoft.AspNetCore.Mvc;
using server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReminderController : ControllerBase
    {
        // GET: api/<ReminerController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }




        // GET api/<ReminerController>/5
        [HttpGet("{email}")]
        public List<Reminder> Get(string email)
        {
			Reminder reminder = new Reminder();
			return reminder.ReadReminderByEmail(email);
		}

        // POST api/<ReminerController>
        [HttpPost]
        public int Post([FromBody] Reminder reminder)
        {
            return reminder.InserReminder();

        }





        // PUT api/<ReminerController>/5
        [HttpPut("{id}")]
        public int Put(int id, [FromBody] Reminder reminder)
        {
			reminder.ReminderID = id;
			return reminder.UpdateReminder();
		}



		// DELETE api/<ReminerController>/5
		[HttpDelete("{id}")]
        public int Delete(int id, [FromBody] Reminder reminder)
        {
			reminder.ReminderID = id;
            return reminder.DeleteReminder();

		}


	}
}
