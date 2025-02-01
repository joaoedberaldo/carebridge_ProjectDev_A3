using CareBridgeBackend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get appointments for a patient (Patients only)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpGet("{id}/appointments")]
        public async Task<IActionResult> GetAppointments(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (userId != id)
                return Unauthorized("You can only view your own appointments.");

            var appointments = await _context.Appointments
                .Where(a => a.PatientId == id)
                .ToListAsync();

            return Ok(appointments);
        }

        /// <summary>
        /// Get diagnostics for a patient (Patients or Doctors)
        /// </summary>
        [Authorize(Roles = "Patient,Doctor")]
        [HttpGet("{id}/diagnostics")]
        public async Task<IActionResult> GetDiagnostics(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole == "Patient" && userId != id)
                return Unauthorized("You can only view your own diagnostics.");

            var diagnostics = await _context.PatientDiagnostics
                .Where(pd => pd.PatientId == id)
                .ToListAsync();

            return Ok(diagnostics);
        }

        [HttpPost("{id}/reviews")]
        public IActionResult PostReview(int id)
        {
            return Ok(new { Message = $"Review added for Patient {id} (mock response)." });
        }
    }
}
