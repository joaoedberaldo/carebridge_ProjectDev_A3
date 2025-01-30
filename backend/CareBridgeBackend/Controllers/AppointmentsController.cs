using CareBridgeBackend.Data;
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
        /// Create a new appointment (Patients only)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
                return BadRequest("Invalid appointment data.");

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Ensure the logged-in user is creating an appointment for themselves
            if (appointment.PatientId != userId)
                return Unauthorized("Patients can only create appointments for themselves.");

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Appointment created successfully.", appointment.Id });
        }

        /// <summary>
        /// Get an appointment (Doctors or Patients)
        /// </summary>
        [Authorize(Roles = "Doctor,Patient")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointment(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound("Appointment not found.");

            return Ok(appointment);
        }

        /// <summary>
        /// Update an appointment (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment updatedAppointment)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found.");

            appointment.AppointmentDate = updatedAppointment.AppointmentDate;
            appointment.Notes = updatedAppointment.Notes;

            await _context.SaveChangesAsync();
            return Ok(new { Message = $"Appointment {id} updated successfully." });
        }

        /// <summary>
        /// Delete an appointment (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound("Appointment not found.");

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Appointment {id} deleted successfully." });
        }
    }
}
