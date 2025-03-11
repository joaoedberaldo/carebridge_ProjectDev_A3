using CareBridgeBackend.Data;
using Microsoft.AspNetCore.Authorization;
using CareBridgeBackend.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CareBridgeBackend.Models;

namespace CareBridgeBackend.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get user details (Authenticated users)
        /// </summary>
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            return Ok(new
            {
                user.Id,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.PhoneNumber,
                user.Specialization,
                user.LicenseNumber
            });
        }

        /// <summary>
        /// Update user profile (Authenticated users)
        /// </summary>
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userId != id && userRole != "Doctor")
                return Unauthorized(new { Message = "You can only update your own profile." });

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { Message = "User not found." });

            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.PhoneNumber = dto.PhoneNumber ?? user.PhoneNumber;

            if (user.Role == UserRole.Doctor) 
            {
                user.Specialization = dto.Specialization ?? user.Specialization;
                user.LicenseNumber = dto.LicenseNumber ?? user.LicenseNumber;
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = "User updated successfully." });
        }

        /// <summary>
        /// Assign an Office to a Doctor (Doctor Id, Office Id)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="officeId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}/assign-office/{officeId}")]
        public async Task<IActionResult> AssignOffice(int id, int officeId)
        {
            var doctor = await _context.Users.FindAsync(id);
            if (doctor == null || doctor.Role != UserRole.Doctor)
                return BadRequest(new { Message = "Invalid doctor ID." });

            var office = await _context.Offices.FindAsync(officeId);
            if (office == null)
                return NotFound(new { Message = "Office not found." });

            doctor.OfficeId = officeId;
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Doctor {id} assigned to Office {officeId}." });
        }
    }
}
