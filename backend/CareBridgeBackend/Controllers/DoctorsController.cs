using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetDoctors()
        {
            return Ok(new[] {
            new { Id = 1, Name = "Dr. Smith", Specialty = "Cardiology" },
            new { Id = 2, Name = "Dr. Jane", Specialty = "Pediatrics" }
        });
        }

        [HttpGet("{id}")]
        public IActionResult GetDoctor(int id)
        {
            return Ok(new { Id = id, Name = "Dr. Smith", Specialty = "Cardiology" });
        }

        [HttpPost("{id}/schedule")]
        public IActionResult AddDoctorSchedule(int id)
        {
            return Ok(new { Message = $"Schedule added for Doctor {id} (mock response)." });
        }

        [HttpPut("{id}/pricing")]
        public IActionResult UpdateDoctorPricing(int id)
        {
            return Ok(new { Message = $"Pricing updated for Doctor {id} (mock response)." });
        }
    }
}
