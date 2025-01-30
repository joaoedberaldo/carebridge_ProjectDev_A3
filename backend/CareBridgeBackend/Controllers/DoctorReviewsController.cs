using CareBridgeBackend.Data;
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
        public async Task<IActionResult> PostReview(int doctorId, [FromBody] DoctorReview review)
        {
            var patientId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (patientId != review.PatientId)
                return Unauthorized("You can only submit reviews for your own appointments.");

            var doctorExists = await _context.Users.AnyAsync(u => u.Id == doctorId && u.Role == UserRole.Doctor);
            if (!doctorExists)
                return NotFound("Doctor not found.");

            review.DoctorId = doctorId;
            review.ReviewDate = DateTime.UtcNow;

            _context.DoctorReviews.Add(review);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Review submitted successfully." });
        }

        /// <summary>
        /// Get all reviews for a doctor (Public)
        /// </summary>
        [HttpGet("{doctorId}")]
        public async Task<IActionResult> GetReviews(int doctorId)
        {
            var reviews = await _context.DoctorReviews
                .Where(r => r.DoctorId == doctorId)
                .ToListAsync();

            return Ok(reviews);
        }
    }
}
