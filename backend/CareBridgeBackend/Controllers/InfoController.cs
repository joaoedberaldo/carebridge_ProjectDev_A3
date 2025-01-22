using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/info")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        [HttpGet("plans")]
        public IActionResult GetPlans()
        {
            return Ok(new[] {
            new { PlanName = "Ontario Health Plan", Description = "Covers basic healthcare." },
            new { PlanName = "Extended Health Plan", Description = "Covers additional services." }
        });
        }
    }
}
