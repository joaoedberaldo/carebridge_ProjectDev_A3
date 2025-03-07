using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareBridgeBackend.Controllers
{
    [Route("api/offices")]
    [ApiController]
    public class OfficesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OfficesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Create Office
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateOffice([FromBody] OfficeCreateDto dto)
        {
            var office = new Office
            {
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode
            };

            _context.Offices.Add(office);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOffice), new { id = office.Id }, new { Message = "Office created successfully.", OfficeId = office.Id });
        }

        /// <summary>
        /// Get Office by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOffice(int id)
        {
            var office = await _context.Offices
                .Include(o => o.Doctors)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (office == null)
                return NotFound(new { Message = "Office not found." });

            var officeDto = new OfficeDto
            {
                Id = office.Id,
                Address = office.Address,
                City = office.City,
                State = office.State,
                ZipCode = office.ZipCode,
                DoctorIds = office.Doctors.Select(d => d.Id).ToList()
            };

            return Ok(officeDto);
        }

        /// <summary>
        /// Update Office
        /// </summary>
        /// <param name="id"></param>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOffice(int id, [FromBody] OfficeCreateDto dto)
        {
            var office = await _context.Offices.FindAsync(id);
            if (office == null)
                return NotFound(new { Message = "Office not found." });

            office.Address = dto.Address;
            office.City = dto.City;
            office.State = dto.State;
            office.ZipCode = dto.ZipCode;

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Office updated successfully." });
        }

        /// <summary>
        /// Delete Office by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOffice(int id)
        {
            var office = await _context.Offices.FindAsync(id);
            if (office == null)
                return NotFound(new { Message = "Office not found." });

            _context.Offices.Remove(office);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Office deleted successfully." });
        }

        /// <summary>
        /// Get all doctors from an Office by Office Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/doctors")]
        public async Task<IActionResult> GetDoctorsInOffice(int id)
        {
            var doctors = await _context.Users
                .Where(u => u.OfficeId == id && u.Role == UserRole.Doctor)
                .Select(u => new { u.Id, u.FirstName, u.LastName, u.Email, u.Specialization })
                .ToListAsync();

            return Ok(doctors);
        }
    }
}
