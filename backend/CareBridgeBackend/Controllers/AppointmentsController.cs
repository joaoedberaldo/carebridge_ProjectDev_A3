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
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create an appointment (Patients only)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Ensure the patient is booking for themselves
            if (userId != dto.PatientId)
                return Unauthorized(new { Message = "Patients can only book appointments for themselves." });

            // Validate doctor existence
            var doctorExists = await _context.Users.AnyAsync(u => u.Id == dto.DoctorId && u.Role == UserRole.Doctor);
            if (!doctorExists)
                return BadRequest(new { Message = "Invalid doctor ID." });

            var appointment = new Appointment
            {
                AppointmentDate = dto.AppointmentDate ?? DateTime.UtcNow, // Default to now if null
                DoctorId = dto.DoctorId,
                PatientId = dto.PatientId,
                Notes = dto.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment created successfully.", AppointmentId = appointment.Id });
        }

        /// <summary>
        /// Get an appointment details (Authenticated users)
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            return Ok(new
            {
                appointment.Id,
                appointment.AppointmentDate,
                Doctor = new { appointment.Doctor.Id, appointment.Doctor.FirstName, appointment.Doctor.LastName, appointment.Doctor.Email },
                Patient = new { appointment.Patient.Id, appointment.Patient.FirstName, appointment.Patient.LastName, appointment.Patient.Email },
                appointment.Notes
            });
        }

        /// <summary>
        /// Update an appointment (Doctors & Assistants only)
        /// </summary>
        [Authorize(Roles = "Doctor,Assistant")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] AppointmentUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            // If user is a doctor, they can update their own appointments
            if (userRole == "Doctor" && appointment.DoctorId == userId)
            {
                appointment.AppointmentDate = dto.AppointmentDate ?? appointment.AppointmentDate;
                appointment.Notes = dto.Notes ?? appointment.Notes;

                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Appointment {id} updated successfully." });
            }

            // If user is an assistant, they can update appointments of doctors they assist
            if (userRole == "Assistant")
            {
                bool isAssignedToDoctor = await _context.DoctorAssistants
                    .AnyAsync(da => da.AssistantId == userId && da.DoctorId == appointment.DoctorId);

                if (!isAssignedToDoctor)
                    return Unauthorized(new { Message = "You are not authorized to manage this doctor's appointments." });

                appointment.AppointmentDate = dto.AppointmentDate ?? appointment.AppointmentDate;
                appointment.Notes = dto.Notes ?? appointment.Notes;

                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Appointment {id} updated successfully by Assistant." });
            }

            return Unauthorized(new { Message = "You are not authorized to update this appointment." });
        }

        /// <summary>
        /// Delete an appointment (Doctors, Assistants, & Patients)
        /// </summary>
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound(new { Message = "Appointment not found." });

            // If user is the doctor assigned to the appointment, they can cancel it
            if (userRole == "Doctor" && appointment.DoctorId == userId)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Appointment {id} cancelled successfully by Doctor." });
            }

            // If user is the patient, they can cancel their own appointment
            if (userRole == "Patient" && appointment.PatientId == userId)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Appointment {id} cancelled successfully by Patient." });
            }

            // If user is an assistant, they can cancel appointments of doctors they assist
            if (userRole == "Assistant")
            {
                bool isAssignedToDoctor = await _context.DoctorAssistants
                    .AnyAsync(da => da.AssistantId == userId && da.DoctorId == appointment.DoctorId);

                if (!isAssignedToDoctor)
                    return Unauthorized(new { Message = "You are not authorized to cancel this doctor's appointments." });

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Appointment {id} cancelled successfully by Assistant." });
            }

            return Unauthorized(new { Message = "You are not authorized to cancel this appointment." });
        }
    }
}
