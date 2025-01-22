using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareBridgeBackend.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        [HttpGet("{id}/appointments")]
        public IActionResult GetAppointments(int id)
        {
            return Ok(new[] {
            new { AppointmentId = 1, DoctorName = "Dr. Smith", Time = "10:00 AM" },
            new { AppointmentId = 2, DoctorName = "Dr. Jane", Time = "2:00 PM" }
        });
        }

        [HttpGet("{id}/diagnostics")]
        public IActionResult GetDiagnostics(int id)
        {
            return Ok(new[] {
            new { DiagnosticId = 1, Result = "Healthy", Date = "2025-01-14" },
            new { DiagnosticId = 2, Result = "High Blood Pressure", Date = "2025-01-15" }
        });
        }

        [HttpPost("{id}/reviews")]
        public IActionResult PostReview(int id)
        {
            return Ok(new { Message = $"Review added for Patient {id} (mock response)." });
        }
    }
}
