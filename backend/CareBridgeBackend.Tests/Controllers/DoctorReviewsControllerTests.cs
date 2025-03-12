using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace CareBridgeBackend.Tests.Controllers
{
    public class DoctorReviewsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DoctorReviewsController _controller;

        public DoctorReviewsControllerTests()
        {
            // Unique InMemoryDatabase for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new DoctorReviewsController(_context);
        }

        /// <summary>
        /// Test: Successfully submits a doctor review.
        /// </summary>
        [Fact]
        public async Task PostReview_Succeeds_WhenValid()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Smith",
                LastName = "Doe",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Patient",
                LastName = "Jones",
                Email = "patient@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(doctor, patient);
            await _context.SaveChangesAsync();

            var reviewDto = new DoctorReviewCreateDto
            {
                Rating = 5,
                ReviewText = "Great doctor!"
            };

            SetFakeHttpContext(_controller, "2", "Patient");

            // Act
            var result = await _controller.PostReview(1, reviewDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);

            // Check that the review was saved in the database
            var reviewExists = await _context.DoctorReviews
                .AnyAsync(r => r.DoctorId == 1 && r.PatientId == 2);
            Assert.True(reviewExists, "Review should exist in the database.");
        }


        /// <summary>
        /// Test: Patients can only review doctors, not other roles.
        /// </summary>
        [Fact]
        public async Task PostReview_Fails_WhenReviewingNonDoctor()
        {
            // Arrange
            var patient1 = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "Patient1",
                LastName = "Doe",
                Email = "patient1@example.com",
                Password = "hashed_password"
            };
            var patient2 = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Patient2",
                LastName = "Smith",
                Email = "patient2@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(patient1, patient2);
            await _context.SaveChangesAsync();

            var reviewDto = new DoctorReviewCreateDto
            {
                Rating = 4,
                ReviewText = "Trying to review a non-doctor"
            };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.PostReview(2, reviewDto) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Doctor not found", result.Value.ToString());
        }

        /// <summary>
        /// Test: Fetching all reviews returns structured data.
        /// </summary>
        [Fact]
        public async Task GetReviews_ReturnsReviews_WhenAvailable()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Brown",
                LastName = "Smith",
                Email = "doctor2@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Password = "hashed_password"
            };

            var review = new DoctorReview
            {
                DoctorId = 1,
                PatientId = 2,
                Rating = 5,
                ReviewText = "Excellent!",
                ReviewDate = DateTime.UtcNow
            };

            _context.Users.AddRange(doctor, patient);
            _context.DoctorReviews.Add(review);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetReviews(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var reviews = result.Value as List<DoctorReviewDto>;
            Assert.NotNull(reviews);
            Assert.Single(reviews);
            Assert.Equal(5, reviews[0].Rating);
            Assert.Equal("Excellent!", reviews[0].ReviewText);
            Assert.Equal("John Doe", reviews[0].PatientName);
        }

        /// <summary>
        /// Test: Submitting a review without required fields should fail.
        /// </summary>
        [Fact]
        public async Task PostReview_Fails_WhenMissingFields()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Williams",
                LastName = "Doe",
                Email = "doctor3@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Patient",
                LastName = "Smith",
                Email = "patient3@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(doctor, patient);
            await _context.SaveChangesAsync();

            var invalidReviewDto = new DoctorReviewCreateDto
            {
                Rating = 0, // Invalid rating (should be between 1-5)
                ReviewText = "" // Missing text
            };

            SetFakeHttpContext(_controller, "2", "Patient");

            // **Manually add ModelState errors**
            _controller.ModelState.AddModelError("Rating", "Rating is required and must be between 1 and 5.");
            _controller.ModelState.AddModelError("ReviewText", "Review text is required.");

            // Act
            var result = await _controller.PostReview(1, invalidReviewDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);

            // **Ensure the response contains validation errors**
            var responseErrors = result.Value as SerializableError;
            Assert.NotNull(responseErrors);
            Assert.True(responseErrors.ContainsKey("Rating"));
            Assert.True(responseErrors.ContainsKey("ReviewText"));
        }


        #region Helpers
        /// <summary>
        /// Helper method to set fake authentication context.
        /// </summary>
        private void SetFakeHttpContext(ControllerBase controller, string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        /// <summary>
        /// Cleanup method to dispose of database context after each test.
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }
        #endregion
    }
}
