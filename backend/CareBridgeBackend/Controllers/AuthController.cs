using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("register")]
        public IActionResult Register()
        {
            return Ok(new { Message = "User registered successfully (mock response)." });
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            return Ok(new { Token = "mock-jwt-token" });
        }
    }
}
