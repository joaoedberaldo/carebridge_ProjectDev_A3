using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            return Ok(new { Id = id, Name = "Mock User", Role = "Patient" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id)
        {
            return Ok(new { Message = $"User {id} updated successfully (mock response)." });
        }
    }
}
