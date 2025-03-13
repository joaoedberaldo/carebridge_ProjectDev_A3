using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace CareBridgeBackend.Tests.Controllers
{
    public class TreatmentControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly TreatmentController _controller;

        public TreatmentControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new TreatmentController(_context);
        }

        /// <summary>
        /// Doctors or assistants can create treatments successfully.
        /// </summary>
        [Fact]
        public async Task CreateTreatment_Succeeds_WhenValid()
        {
            // Arrange
            var diagnostic = new PatientDiagnostic { Id = 1, PatientId = 2, DiagnosticTemplateId = 100, DateDiagnosed = DateTime.UtcNow, Notes = "Flu Test" };
            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            var treatmentDto = new TreatmentCreateDto
            {
                Name = "Pain Management",
                Description = "Take medication twice a day.",
                PatientDiagnosticId = 1,
                StartDate = DateTime.UtcNow
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.CreateTreatment(treatmentDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value.GetType()
                .GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value)?.ToString());

            Assert.Contains("Message", response.Keys);
            Assert.Contains("Treatment created successfully.", response.Values);
        }

        /// <summary>
        /// Creating a treatment fails when the diagnostic ID does not exist.
        /// </summary>
        [Fact]
        public async Task CreateTreatment_ReturnsBadRequest_WhenDiagnosticDoesNotExist()
        {
            // Arrange
            var treatmentDto = new TreatmentCreateDto
            {
                Name = "Physical Therapy",
                Description = "Daily stretching exercises.",
                PatientDiagnosticId = 999, // Non-existent ID
                StartDate = DateTime.UtcNow
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.CreateTreatment(treatmentDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("The provided PatientDiagnosticId does not exist", result.Value.ToString());
        }

        /// <summary>
        /// Retrieves treatments for a valid patient diagnostic.
        /// </summary>
        [Fact]
        public async Task GetTreatmentsForDiagnostic_ReturnsTreatments_WhenValid()
        {
            // Arrange
            var diagnostic = new PatientDiagnostic { Id = 1, PatientId = 2, DiagnosticTemplateId = 100, DateDiagnosed = DateTime.UtcNow, Notes = "Flu Test" };
            var treatment = new Treatment { Id = 1, Name = "Rest", Description = "Bed rest for three days", PatientDiagnosticId = 1, StartDate = DateTime.UtcNow };

            _context.PatientDiagnostics.Add(diagnostic);
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTreatmentsForDiagnostic(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var treatments = Assert.IsType<List<TreatmentDto>>(result.Value);
            Assert.Single(treatments);
        }

        /// <summary>
        /// Updating a treatment successfully modifies its details.
        /// </summary>
        [Fact]
        public async Task UpdateTreatment_Succeeds_WhenValid()
        {
            // Arrange
            var treatment = new Treatment { Id = 1, Name = "Initial Treatment", Description = "Original Description", PatientDiagnosticId = 1, StartDate = DateTime.UtcNow };
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();

            var updateDto = new TreatmentUpdateDto
            {
                Name = "Updated Treatment",
                Description = "New treatment description"
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.UpdateTreatment(1, updateDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Treatment updated successfully", result.Value.ToString());

            // Verify data was updated
            var updatedTreatment = await _context.Treatments.FindAsync(1);
            Assert.Equal("Updated Treatment", updatedTreatment.Name);
            Assert.Equal("New treatment description", updatedTreatment.Description);
        }

        /// <summary>
        /// Updating a treatment fails when the treatment does not exist.
        /// </summary>
        [Fact]
        public async Task UpdateTreatment_ReturnsNotFound_WhenTreatmentDoesNotExist()
        {
            // Arrange
            var updateDto = new TreatmentUpdateDto
            {
                Name = "Non-Existent Update",
                Description = "This shouldn't work."
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.UpdateTreatment(999, updateDto) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Treatment not found", result.Value.ToString());
        }

        /// <summary>
        /// Deleting a treatment successfully removes it.
        /// </summary>
        [Fact]
        public async Task DeleteTreatment_Succeeds_WhenValid()
        {
            // Arrange
            var treatment = new Treatment { Id = 1, Name = "Bed Rest", Description = "Stay in bed for 3 days", PatientDiagnosticId = 1, StartDate = DateTime.UtcNow };
            _context.Treatments.Add(treatment);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.DeleteTreatment(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.DoesNotContain(_context.Treatments, t => t.Id == 1);
        }

        /// <summary>
        /// Deleting a treatment fails when it does not exist.
        /// </summary>
        [Fact]
        public async Task DeleteTreatment_ReturnsNotFound_WhenTreatmentDoesNotExist()
        {
            // Arrange
            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.DeleteTreatment(999) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Treatment not found", result.Value.ToString());
        }

        /// <summary>
        /// Helper method to set the fake HTTP context with user authentication.
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
    }
}
