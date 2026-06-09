using server.BL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Hw1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VacationsController : ControllerBase
    {
        // GET: api/<VacationsController>
        [HttpGet]
        public IEnumerable<Vacation> Get()
        {
            Vacation v = new Vacation();
            return v.ReadVacation();
        }


        [HttpGet("GetBySEDate")]
        public IEnumerable<Vacation> GetBySEDate(DateTime SD, DateTime ED)
        {
            Vacation v = new Vacation();
            return v.readBySEDate(SD, ED);
        }

        // GET api/<VacationsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        // POST api/<VacationsController>
        [HttpPost]
        public int Post([FromBody] Vacation v)
        {
            return v.InsertVacation();
        }

        // PUT api/<VacationsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<VacationsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        // GET: api/<FlatsController>- שמתי את זה בהערה זה לא הצלחתי לדבג
        [HttpGet("priceByMonth")]
        public object getAveragePricePerNight(int month)
        {
            Vacation vacation = new Vacation();
            return vacation.getAVG(month);
        }

        //GET api/<FlatsController>/5
        //[HttpGet("{id}")]
        //public int ReadFlatbyID(int id)
        //{

        //    return 0;


        //}
    }
}
