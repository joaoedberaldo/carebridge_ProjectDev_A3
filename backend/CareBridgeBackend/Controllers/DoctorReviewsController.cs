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
    [Route("api/reviews")]
    [ApiController]
    public class DoctorReviewsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DoctorReviewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Submit a doctor review (Patients only)
        /// </summary>
        [Authorize(Roles = "Patient")]
        [HttpPost("{doctorId}")]
        public async Task<IActionResult> PostReview(int doctorId, [FromBody] DoctorReviewCreateDto reviewDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Return validation errors

            var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var doctorExists = await _context.Users.AnyAsync(u => u.Id == doctorId && u.Role == UserRole.Doctor);
            if (!doctorExists)
                return NotFound(new { Message = "Doctor not found." });

            var review = new DoctorReview
            {
                DoctorId = doctorId,
                PatientId = patientId,  // Take patientId from token, not DTO
                Rating = reviewDto.Rating,
                ReviewText = reviewDto.ReviewText,
                ReviewDate = DateTime.UtcNow
            };

            _context.DoctorReviews.Add(review);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReviews), new { doctorId = doctorId }, new { Message = "Review submitted successfully." });
        }




        /// <summary>
        /// Get all reviews for a doctor (Public)
        /// </summary>
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetReviews(int doctorId)
        {
            var reviews = await _context.DoctorReviews
                .Where(r => r.DoctorId == doctorId)
                .Include(r => r.Patient)
                .Select(r => new DoctorReviewDto
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    ReviewText = r.ReviewText,
                    PatientId = r.PatientId,
                    PatientName = r.Patient.FirstName + " " + r.Patient.LastName,
                    ReviewDate = r.ReviewDate
                })
                .ToListAsync();

            return Ok(reviews);
        }
    }
}
