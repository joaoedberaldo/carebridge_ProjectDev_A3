using CareBridgeBackend.Data;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Controllers
{
    [Route("api/doctors/schedules")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    public class DoctorSchedulesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorSchedulesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get schedules for the logged-in doctor.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMySchedules()
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var schedules = await _context.DoctorSchedules
                .Where(ds => ds.DoctorId == doctorId)
                .ToListAsync();

            return Ok(schedules);
        }

        /// <summary>
        /// Create a new schedule entry for the logged-in doctor.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] DoctorSchedule schedule)
        {
            if (schedule == null)
                return BadRequest("Invalid schedule data.");

            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // Ensure the schedule is created for the logged-in doctor.
            schedule.DoctorId = doctorId;

            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Schedule created successfully.", ScheduleId = schedule.Id });
        }

        /// <summary>
        /// Update an existing schedule entry for the logged-in doctor.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] DoctorSchedule updatedSchedule)
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound("Schedule not found.");

            if (schedule.DoctorId != doctorId)
                return Unauthorized("You are not authorized to update this schedule.");

            // Update allowed fields.
            schedule.StartTime = updatedSchedule.StartTime;
            schedule.EndTime = updatedSchedule.EndTime;
            schedule.Description = updatedSchedule.Description;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Schedule updated successfully." });
        }

        /// <summary>
        /// Delete a schedule entry for the logged-in doctor.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var schedule = await _context.DoctorSchedules.FindAsync(id);
            if (schedule == null)
                return NotFound("Schedule not found.");

            if (schedule.DoctorId != doctorId)
                return Unauthorized("You are not authorized to delete this schedule.");

            _context.DoctorSchedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Schedule deleted successfully." });
        }
    }
}
