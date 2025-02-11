using CareBridgeBackend.Data;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/assistants")]
    [ApiController]
    public class AssistantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AssistantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get the list of doctors an assistant is assigned to (Assistants only)
        /// </summary>
        [Authorize(Roles = "Assistant")]
        [HttpGet("my-doctors")]
        public async Task<IActionResult> GetMyDoctors()
        {
            var assistantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var doctors = await _context.DoctorAssistants
                .Where(da => da.AssistantId == assistantId)
                .Select(da => new
                {
                    da.Doctor.Id,
                    da.Doctor.FirstName,
                    da.Doctor.LastName,
                    da.Doctor.Specialization
                })
                .ToListAsync();

            return Ok(doctors);
        }

        /// <summary>
        /// Assign an assistant to a doctor (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost("{assistantId}/assign")]
        public async Task<IActionResult> AssignAssistant(int assistantId)
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var assistant = await _context.Users.FindAsync(assistantId);
            if (assistant == null || assistant.Role != UserRole.Assistant)
                return BadRequest(new { Message = "Invalid assistant." });

            var existingAssignment = await _context.DoctorAssistants
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.AssistantId == assistantId);

            if (existingAssignment != null)
                return BadRequest(new { Message = "Assistant is already assigned to this doctor." });

            var newAssignment = new DoctorAssistant
            {
                DoctorId = doctorId,
                AssistantId = assistantId
            };

            _context.DoctorAssistants.Add(newAssignment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Assistant assigned successfully." });
        }

        /// <summary>
        /// Remove an assistant from a doctor (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("{assistantId}/remove")]
        public async Task<IActionResult> RemoveAssistant(int assistantId)
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var assignment = await _context.DoctorAssistants
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.AssistantId == assistantId);

            if (assignment == null)
                return NotFound(new { Message = "No assignment found for the specified assistant and doctor." });

            _context.DoctorAssistants.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Assistant removed successfully." });
        }

        /// <summary>
        /// Get patient appointments for the doctor the assistant is assigned to (Assistants only)
        /// </summary>
        [Authorize(Roles = "Assistant")]
        [HttpGet("{doctorId}/appointments")]
        public async Task<IActionResult> GetAppointments(int doctorId)
        {
            var assistantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var isAssigned = await _context.DoctorAssistants
                .AnyAsync(da => da.DoctorId == doctorId && da.AssistantId == assistantId);

            if (!isAssigned)
                return Unauthorized(new { Message = "You are not assigned to this doctor." });

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .ToListAsync();

            return Ok(appointments);
        }
    }
}
