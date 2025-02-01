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
    [Route("api/diagnostictemplates")]
    [ApiController]
    public class DiagnosticTemplateController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticTemplateController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all diagnostic templates (Public)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            var templates = await _context.DiagnosticTemplates
                .Include(dt => dt.CreatedByDoctor)
                .ToListAsync();
            return Ok(templates);
        }

        /// <summary>
        /// Get a diagnostic template by ID (Public)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplate(int id)
        {
            var template = await _context.DiagnosticTemplates
                .Include(dt => dt.CreatedByDoctor)
                .FirstOrDefaultAsync(dt => dt.Id == id);

            if (template == null)
                return NotFound("Diagnostic template not found.");

            return Ok(template);
        }

        /// <summary>
        /// Create a new diagnostic template (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPost]
        public async Task<IActionResult> CreateTemplate([FromBody] DiagnosticTemplateCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the logged-in doctor’s ID from the token
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var template = new DiagnosticTemplate
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedByDoctorId = doctorId
            };

            _context.DiagnosticTemplates.Add(template);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Diagnostic template created successfully.", TemplateId = template.Id });
        }

        /// <summary>
        /// Update an existing diagnostic template (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] DiagnosticTemplateUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var template = await _context.DiagnosticTemplates.FindAsync(id);
            if (template == null)
                return NotFound("Diagnostic template not found.");

            // Ensure the logged-in doctor is the creator of the template.
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (template.CreatedByDoctorId != doctorId)
                return Unauthorized("You are not authorized to update this template.");

            template.Name = dto.Name;
            template.Description = dto.Description;

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Diagnostic template updated successfully." });
        }

        /// <summary>
        /// Delete a diagnostic template (Doctors only)
        /// </summary>
        [Authorize(Roles = "Doctor")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTemplate(int id)
        {
            var template = await _context.DiagnosticTemplates.FindAsync(id);
            if (template == null)
                return NotFound("Diagnostic template not found.");

            // Ensure the logged-in doctor is the creator of the template.
            var doctorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (template.CreatedByDoctorId != doctorId)
                return Unauthorized("You are not authorized to delete this template.");

            _context.DiagnosticTemplates.Remove(template);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Diagnostic template deleted successfully." });
        }
    }
}
