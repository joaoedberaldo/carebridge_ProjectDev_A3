using CareBridgeBackend.Data;
using CareBridgeBackend.Models;
using CareBridgeBackend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/medicalhistory")]
    [ApiController]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MedicalHistoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create a new MedicalHistory for a patient
        /// </summary>
        [Authorize(Roles = "Doctor,Assistant")]
        [HttpPost]
        public async Task<IActionResult> AddMedicalHistory([FromBody] MedicalHistoryCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Ensure the patient exists
            var patientExists = await _context.Users.AnyAsync(u => u.Id == dto.PatientId && u.Role == UserRole.Patient);
            if (!patientExists)
                return NotFound(new { Message = "Patient not found." });

            var history = new MedicalHistory
            {
                PatientId = dto.PatientId
            };

            _context.MedicalHistories.Add(history);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Medical history created successfully.", MedicalHistoryId = history.Id });
        }

        /// <summary>
        /// Verify if a patient has a MedicalHistory, return MedicalHistory ID or 0
        /// </summary>
        [HttpGet("verify/{patientId}")]
        public async Task<IActionResult> VerifyMedicalHistory(int patientId)
        {
            var history = await _context.MedicalHistories
                .Where(mh => mh.PatientId == patientId)
                .Select(mh => mh.Id)
                .FirstOrDefaultAsync();

            return Ok(new { MedicalHistoryId = history > 0 ? history : 0 });
        }

        /// <summary>
        /// Get MedicalHistory by Patient ID (Patients and Doctors)
        /// </summary>
        [Authorize(Roles = "Patient,Doctor")]
        [HttpGet("{patientId}")]
        public async Task<IActionResult> GetMedicalHistory(int patientId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Ensure patients can only view their own medical history
            if (userRole == "Patient" && userId != patientId)
                return Unauthorized(new { Message = "You can only view your own medical history." });

            var medicalHistory = await _context.MedicalHistories
                .Include(mh => mh.Appointments)
                .Include(mh => mh.PatientDiagnostics)
                .FirstOrDefaultAsync(mh => mh.PatientId == patientId);

            if (medicalHistory == null)
                return NotFound(new { Message = "Medical history not found for this patient." });

            var historyDto = new MedicalHistoryDto
            {
                Id = medicalHistory.Id,
                PatientId = medicalHistory.PatientId,
                Appointments = medicalHistory.Appointments.Select(a => new AppointmentDto
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    Notes = a.Notes
                }).ToList(),
                PatientDiagnostics = medicalHistory.PatientDiagnostics.Select(pd => new PatientDiagnosticDto
                {
                    Id = pd.Id,
                    DiagnosticTemplateId = pd.DiagnosticTemplateId,
                    DateDiagnosed = pd.DateDiagnosed,
                    Notes = pd.Notes
                }).ToList()
            };

            return Ok(historyDto);
        }

        /// <summary>
        /// Delete MedicalHistory by Patient ID (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("{patientId}")]
        public async Task<IActionResult> DeleteMedicalHistory(int patientId)
        {
            var medicalHistory = await _context.MedicalHistories
                .FirstOrDefaultAsync(mh => mh.PatientId == patientId);

            if (medicalHistory == null)
                return NotFound(new { Message = "Medical history not found." });

            _context.MedicalHistories.Remove(medicalHistory);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Medical history deleted successfully." });
        }
    }
}
