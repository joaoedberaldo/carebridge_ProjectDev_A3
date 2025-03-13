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
using System.Threading.Tasks;
using System.Linq;

namespace CareBridgeBackend.Tests.Controllers
{
    public class DiagnosticTemplateControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DiagnosticTemplateController _controller;

        public DiagnosticTemplateControllerTests()
        {
            // Unique InMemoryDatabase for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new DiagnosticTemplateController(_context);
        }

        /// <summary>
        /// Test: Should return all diagnostic templates.
        /// </summary>
        [Fact]
        public async Task GetAllTemplates_ReturnsTemplates()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "doctor@example.com",  
                Password = "hashed_password",  
                Role = UserRole.Doctor
            };
            var template1 = new DiagnosticTemplate { Id = 1, Name = "Template 1", Description = "Desc 1", CreatedByDoctorId = 1, CreatedByDoctor = doctor };
            var template2 = new DiagnosticTemplate { Id = 2, Name = "Template 2", Description = "Desc 2", CreatedByDoctorId = 1, CreatedByDoctor = doctor };

            _context.Users.Add(doctor);
            _context.DiagnosticTemplates.AddRange(template1, template2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetAllTemplates() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var templates = result.Value as List<DiagnosticTemplateDto>;
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);
        }

        /// <summary>
        /// Test: Should return a diagnostic template by ID.
        /// </summary>
        [Fact]
        public async Task GetTemplate_ReturnsTemplate_WhenExists()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "doctor@example.com",  
                Password = "hashed_password",  
                Role = UserRole.Doctor
            };
            var template = new DiagnosticTemplate { Id = 1, Name = "Test Template", Description = "Test Desc", CreatedByDoctorId = 1, CreatedByDoctor = doctor };

            _context.Users.Add(doctor);
            _context.DiagnosticTemplates.Add(template);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetTemplate(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var response = result.Value as DiagnosticTemplateDto;
            Assert.NotNull(response);
            Assert.Equal("Test Template", response.Name);
            Assert.Equal("John Doe", response.CreatedByDoctorName);
        }

        /// <summary>
        /// Test: Should return 404 when template does not exist.
        /// </summary>
        [Fact]
        public async Task GetTemplate_ReturnsNotFound_WhenDoesNotExist()
        {
            // Act
            var result = await _controller.GetTemplate(99) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("not found", result.Value.ToString());
        }

        /// <summary>
        /// Test: Should create a diagnostic template when valid (Doctor only).
        /// </summary>
        [Fact]
        public async Task CreateTemplate_Succeeds_WhenValid()
        {
            // Arrange
            SetFakeHttpContext(_controller, "1", "Doctor");
            var dto = new DiagnosticTemplateCreateDto { Name = "New Template", Description = "New Desc" };

            // Act
            var result = await _controller.CreateTemplate(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Check DB
            var templateExists = await _context.DiagnosticTemplates.AnyAsync(dt => dt.Name == "New Template");
            Assert.True(templateExists);
        }

        /// <summary>
        /// Test: Should update a template if doctor is creator.
        /// </summary>
        [Fact]
        public async Task UpdateTemplate_Succeeds_WhenDoctorIsCreator()
        {
            // Arrange
            var template = new DiagnosticTemplate { Id = 1, Name = "Old", Description = "Old Desc", CreatedByDoctorId = 1 };
            _context.DiagnosticTemplates.Add(template);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            var dto = new DiagnosticTemplateUpdateDto { Name = "Updated", Description = "Updated Desc" };

            // Act
            var result = await _controller.UpdateTemplate(1, dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updated = await _context.DiagnosticTemplates.FindAsync(1);
            Assert.Equal("Updated", updated.Name);
        }

        /// <summary>
        /// Test: Should return Unauthorized if doctor is not creator.
        /// </summary>
        [Fact]
        public async Task UpdateTemplate_ReturnsUnauthorized_WhenNotCreator()
        {
            // Arrange
            var template = new DiagnosticTemplate { Id = 1, Name = "Old", Description = "Old Desc", CreatedByDoctorId = 2 };
            _context.DiagnosticTemplates.Add(template);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            var dto = new DiagnosticTemplateUpdateDto { Name = "Updated", Description = "Updated Desc" };

            // Act
            var result = await _controller.UpdateTemplate(1, dto) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        #region Helpers
        private void SetFakeHttpContext(ControllerBase controller, string userId, string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };
        }

        public void Dispose() => _context.Dispose();
        #endregion
    }
}
