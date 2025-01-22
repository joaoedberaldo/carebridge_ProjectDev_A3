using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateAppointment()
        {
            return Ok(new { Message = "Appointment created successfully (mock response)." });
        }

        [HttpGet("{id}")]
        public IActionResult GetAppointment(int id)
        {
            return Ok(new { Id = id, DoctorName = "Dr. Smith", Time = "10:00 AM" });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAppointment(int id)
        {
            return Ok(new { Message = $"Appointment {id} updated successfully (mock response)." });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAppointment(int id)
        {
            return Ok(new { Message = $"Appointment {id} deleted successfully (mock response)." });
        }
    }
}
