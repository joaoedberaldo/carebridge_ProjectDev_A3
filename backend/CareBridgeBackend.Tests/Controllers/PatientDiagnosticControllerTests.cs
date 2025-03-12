using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareBridgeBackend.Tests.Controllers
{
    public class PatientDiagnosticControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientDiagnosticController _controller;

        public PatientDiagnosticControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PatientDiagnosticController(_context);
        }

        /// <summary>
        /// Test: Get diagnostics for a patient returns only their records.
        /// </summary>
        [Fact]
        public async Task GetPatientDiagnostics_ReturnsDiagnostics_WhenAuthorized()
        {
            // Arrange
            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashed_password"
            };

            var diagnostic = new PatientDiagnostic
            {
                Id = 1,
                DiagnosticTemplateId = 100,
                PatientId = 1,
                DoctorId = 2,
                DateDiagnosed = DateTime.UtcNow,
                Notes = "Mild symptoms"
            };

            _context.Users.Add(patient);
            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient"); // Mock authentication

            // Act
            var result = await _controller.GetPatientDiagnostics(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as List<PatientDiagnostic>;
            Assert.NotNull(response);
            Assert.Single(response);
            Assert.Equal(1, response[0].Id);
        }

        /// <summary>
        /// Test: Get diagnostics fails if a patient tries to access another patient's data.
        /// </summary>
        [Fact]
        public async Task GetPatientDiagnostics_ReturnsUnauthorized_WhenAccessingOthers()
        {
            // Arrange: Create two patients
            var patient1 = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashed_password"
            };

            var patient2 = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice@example.com",
                Password = "hashed_password"
            };

            var diagnosticTemplate = new DiagnosticTemplate
            {
                Id = 100,
                Name = "General Checkup",
                Description = "Routine diagnostic for annual checkup"
            };

            var diagnostic = new PatientDiagnostic
            {
                Id = 1,
                DiagnosticTemplateId = 100,
                PatientId = 1, 
                DoctorId = 3,
                DateDiagnosed = DateTime.UtcNow,
                Notes = "Mild symptoms"
            };

            // Save required data
            _context.Users.AddRange(patient1, patient2);
            _context.DiagnosticTemplates.Add(diagnosticTemplate);
            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            // Set Patient 2 as the logged-in user
            SetFakeHttpContext(_controller, "2", "Patient");

            // Act
            var result = await _controller.GetPatientDiagnostics(1);

            // Assert (Simplified)
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        /// <summary>
        /// Test: Creating a diagnostic succeeds when valid.
        /// </summary>
        [Fact]
        public async Task CreateDiagnostic_CreatesNewDiagnostic_WhenValid()
        {
            // Arrange
            var doctor = new User
            {
                Id = 2,
                Role = UserRole.Doctor,
                FirstName = "Dr.",
                LastName = "Smith",
                Email = "drsmith@example.com",
                Password = "hashed_password"
            };

            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashed_password"
            };

            var diagnosticTemplate = new DiagnosticTemplate
            {
                Id = 100,
                Name = "Flu Test",
                Description = "Test for influenza"
            };

            _context.Users.AddRange(doctor, patient);
            _context.DiagnosticTemplates.Add(diagnosticTemplate);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "2", "Doctor"); 

            var dto = new PatientDiagnosticCreateDto
            {
                DiagnosticTemplateId = 100,
                PatientId = 1,
                DateDiagnosed = DateTime.UtcNow,
                Notes = "Severe headache"
            };

            // Act
            var result = await _controller.CreateDiagnostic(dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Verify the diagnostic was saved in the database
            var savedDiagnostic = await _context.PatientDiagnostics.FirstOrDefaultAsync();
            Assert.NotNull(savedDiagnostic);
            Assert.Equal(dto.PatientId, savedDiagnostic.PatientId);
            Assert.Equal(dto.DiagnosticTemplateId, savedDiagnostic.DiagnosticTemplateId);
        }



        /// <summary>
        /// Test: Creating a diagnostic fails if user is not a doctor.
        /// </summary>
        [Fact]
        public async Task PatientCannotDiagnoseAnotherPatient()
        {
            // Arrange
            var patient1 = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = "hashed_password"
            };

            var patient2 = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(patient1, patient2);
            await _context.SaveChangesAsync();

            // Simulate login as Patient 1
            var httpContext = new DefaultHttpContext();
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),  
                new Claim(ClaimTypes.Role, "Patient")       
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Create diagnostic request for Patient2
            var dto = new PatientDiagnosticCreateDto
            {
                DiagnosticTemplateId = 100, 
                PatientId = 2,              
                DateDiagnosed = DateTime.UtcNow,
                Notes = "Attempted unauthorized diagnosis"
            };

            // Act
            var result = await _controller.CreateDiagnostic(dto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<UnauthorizedObjectResult>(result);

            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.Equal(401, unauthorizedResult.StatusCode);
            Assert.Contains("Only doctors can create diagnostics", unauthorizedResult.Value.ToString());
        }

        /// <summary>
        /// Test: Creating a diagnostic fails if required fields are missing.
        /// </summary>
        [Fact]
        public async Task CreateDiagnostic_ReturnsBadRequest_WhenInvalidData()
        {
            // Arrange
            SetFakeHttpContext(_controller, "2", "Doctor"); 

            var dto = new PatientDiagnosticCreateDto
            {
                // Missing required fields: DiagnosticTemplateId, PatientId
                DateDiagnosed = DateTime.UtcNow,
                Notes = "Some notes"
            };

            _controller.ModelState.AddModelError("DiagnosticTemplateId", "Required");
            _controller.ModelState.AddModelError("PatientId", "Required");

            // Act
            var result = await _controller.CreateDiagnostic(dto) as BadRequestObjectResult;


            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        /// <summary>
        /// Helper Method: Simulate authentication
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
    }
}
