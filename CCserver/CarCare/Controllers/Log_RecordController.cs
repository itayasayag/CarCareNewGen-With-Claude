using Microsoft.AspNetCore.Mvc;
using server.BL;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Log_RecordController : ControllerBase
    {
        // GET: api/<Log_RecordController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

		// GET api/<Log_RecordController>/5
		[HttpGet("{useremail}")]
		public List<Log_Record> GetCarUserLog(string useremail)
        {
			Log_Record log = new Log_Record();
			return log.GetCarUserLog(useremail);
		}

		[HttpGet("ReadLogRecordByID/{id}")]
		public List<Log_Record> ReadLogRecordByID(int LogID)
		{
			Log_Record log = new Log_Record();
			return log.ReadLogRecord(LogID);
		}



		// POST api/<Log_RecordController>
		[HttpPost]
        public int Post([FromBody] Log_Record logrecord)
        {
            return logrecord.InsertLog_Record();

        }


        // PUT api/<Log_RecordController>/5
        [HttpPut("{id}")]
        public int Put(int id, [FromBody] Log_Record logRecord)
        {
			logRecord.LogID = id;
            return logRecord.UpdateLog();

		}

		// DELETE api/<Log_RecordController>/5
		[HttpDelete("{logid}")]
        public int Delete(int logid, [FromBody] Log_Record logRecord)
        {
			logRecord.LogID = logid;
            return logRecord.DeleteLog();

		}

	}
}
