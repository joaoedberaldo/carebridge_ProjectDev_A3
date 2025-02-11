using CareBridgeBackend.Data;
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

        [HttpPost("{id}/schedule")]
        public IActionResult AddDoctorSchedule(int id)
        {
            return Ok(new { Message = $"Schedule added for Doctor {id} (mock response)." });
        }
        
    }
}
