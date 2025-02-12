using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/diagnostics")]
    [ApiController]
    public class PatientDiagnosticController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PatientDiagnosticController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a patient's diagnostics (Patients only)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetPatientDiagnostics(int patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userId != patientId)
                return Unauthorized(new { Message = "You can only view your own diagnostics." });

            var diagnostics = await _context.PatientDiagnostics
                .Where(pd => pd.PatientId == patientId)
                .ToListAsync();

            return Ok(diagnostics);
        }

        /// <summary>
        /// Create a new diagnostic (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> CreateDiagnostic([FromBody] PatientDiagnosticCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the logged-in doctor's ID
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var diagnostic = new PatientDiagnostic
            {
                DiagnosticTemplateId = dto.DiagnosticTemplateId,
                PatientId = dto.PatientId,
                DoctorId = doctorId,  // Set the creating doctor from the token
                DateDiagnosed = dto.DateDiagnosed,
                Notes = dto.Notes
            };

            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Diagnostic added successfully.", DiagnosticId = diagnostic.Id });
        }

    }
}
