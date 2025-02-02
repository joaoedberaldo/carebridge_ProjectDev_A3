using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
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
                .Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Notes = a.Notes
                })
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
                .Select(pd => new PatientDiagnosticDto
                {
                    Id = pd.Id,
                    DiagnosticTemplateId = pd.DiagnosticTemplateId,
                    DateDiagnosed = pd.DateDiagnosed,
                    Notes = pd.Notes
                })
                .ToListAsync();

            return Ok(diagnostics);
        }

        /// <summary>
        /// Get the full medical history for a patient (Patients or Doctors)
        /// </summary>
        [Authorize(Roles = "Patient,Doctor")]
        [HttpGet("{id}/medicalhistory")]
        public async Task<IActionResult> GetMedicalHistory(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // If the requester is a patient, they can only view their own medical history.
            if (userRole == "Patient" && userId != id)
                return Unauthorized("You can only view your own medical history.");

            // Retrieve the medical history for the given patient id.
            var history = await _context.MedicalHistories
                .Include(mh => mh.Appointments)
                .Include(mh => mh.PatientDiagnostics)
                .Where(mh => mh.PatientId == id)
                .Select(mh => new MedicalHistoryDto
                {
                    Id = mh.Id,
                    PatientId = mh.PatientId,
                    Appointments = mh.Appointments.Select(a => new AppointmentDto
                    {
                        Id = a.Id,
                        AppointmentDate = a.AppointmentDate,
                        Notes = a.Notes
                    }).ToList(),
                    Diagnostics = mh.PatientDiagnostics.Select(pd => new PatientDiagnosticDto
                    {
                        Id = pd.Id,
                        DiagnosticTemplateId = pd.DiagnosticTemplateId,
                        DateDiagnosed = pd.DateDiagnosed,
                        Notes = pd.Notes
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (history == null)
                return NotFound("Medical history not found for the specified patient.");

            return Ok(history);
        }

        [HttpPost("{id}/reviews")]
        public IActionResult PostReview(int id)
        {
            return Ok(new { Message = $"Review added for Patient {id} (mock response)." });
        }
    }
}
