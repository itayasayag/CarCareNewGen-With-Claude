using server.BL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		// GET: api/<UserController>
		[HttpGet]
		public IEnumerable<User> Get()
		{

			User user = new User();
			return user.ReadUser();
		}

		

		// POST api/<UserController>
		[HttpPost]
		public int Post([FromBody] User user)
		{
			return user.Insert();
		}

		// Login api/<UsersController>
        [HttpPost("{email}")]
        public User PostLogin([FromBody] User user)
        {
			return user.Login();

        }

		// PUT api/<UserController>/5
		[HttpPut("{email}")]
		public int Put(string email, [FromBody] User user)
		{
			user.Email = email;
		  return user.Update();

		}

		// DELETE api/<UserController>/5
		[HttpDelete("{email}")]
		public int Delete(string email, [FromBody] User user)
		{
			user.Email=email;
			return user.Delete();
		}

	}
}
