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
    [Route("api/doctors")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // <summary>
        /// Get all doctors (Public)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDoctors()
        {
            var doctors = await _context.Users
                .Where(u => u.Role == UserRole.Doctor)
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Specialization,
                    d.LicenseNumber
                })
                .ToListAsync();

            return Ok(doctors);
        }

        /// <summary>
        /// Get a doctor by ID (Authenticated users)
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _context.Users
                .Where(u => u.Role == UserRole.Doctor && u.Id == id)
                .Select(d => new
                {
                    d.Id,
                    d.FirstName,
                    d.LastName,
                    d.Specialization,
                    d.LicenseNumber
                })
                .FirstOrDefaultAsync();

            if (doctor == null)
                return NotFound(new { Message = "Doctor not found." });

            return Ok(doctor);
        }

        /// <summary>
        /// Get all appointments by doctor ID
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpGet("appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var appointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
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
        /// Get all patients related to the current doctor
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpGet("patients")]
        public async Task<IActionResult> GetMyPatients()
        {
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var patientIds = await _context.Appointments
                .Where(a => a.DoctorId == doctorId)
                .Select(a => a.PatientId)
                .Distinct()
                .ToListAsync();

            var patients = await _context.Users
                .Where(u => patientIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.PhoneNumber
                })
                .ToListAsync();

            return Ok(patients);
        }
    }
}
