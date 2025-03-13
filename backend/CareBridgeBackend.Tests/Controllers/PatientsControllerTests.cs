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
    public class PatientsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PatientsController _controller;

        public PatientsControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PatientsController(_context);
        }

        /// <summary>
        /// Patients can retrieve their own appointments.
        /// </summary>
        [Fact]
        public async Task GetAppointments_ReturnsAppointments_WhenPatientOwnsThem()
        {
            // Arrange
            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var appointment = new Appointment { Id = 1, PatientId = 1, AppointmentDate = DateTime.UtcNow, Notes = "Check-up" };
            _context.Users.Add(patient);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetAppointments(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var appointments = Assert.IsType<List<AppointmentDto>>(result.Value);
            Assert.Single(appointments);
        }

        /// <summary>
        /// Patients cannot retrieve another patient's appointments.
        /// </summary>
        [Fact]
        public async Task GetAppointments_ReturnsUnauthorized_WhenAccessingOthers()
        {
            // Arrange
            var patient1 = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var patient2 = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Password = "hashed_password"
            };
            _context.Users.AddRange(patient1, patient2);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetAppointments(2) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        /// <summary>
        /// Patients can retrieve their own diagnostics.
        /// </summary>
        [Fact]
        public async Task GetDiagnostics_ReturnsDiagnostics_WhenPatientOwnsThem()
        {
            // Arrange
            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var diagnostic = new PatientDiagnostic { Id = 1, PatientId = 1, DiagnosticTemplateId = 100, DateDiagnosed = DateTime.UtcNow, Notes = "Flu Test" };
            _context.Users.Add(patient);
            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetDiagnostics(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var diagnostics = Assert.IsType<List<PatientDiagnosticDto>>(result.Value);
            Assert.Single(diagnostics);
        }

        /// <summary>
        /// Patients cannot retrieve another patient's diagnostics.
        /// </summary>
        [Fact]
        public async Task GetDiagnostics_ReturnsUnauthorized_WhenAccessingOthers()
        {
            // Arrange
            var patient1 = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var patient2 = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Password = "hashed_password"
            };
            _context.Users.AddRange(patient1, patient2);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetDiagnostics(2) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        /// <summary>
        /// Doctors can retrieve any patient's diagnostics.
        /// </summary>
        [Fact]
        public async Task GetDiagnostics_ReturnsDiagnostics_WhenDoctorAccessesPatient()
        {
            // Arrange
            var doctor = new User
            {
                Id = 10,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var diagnostic = new PatientDiagnostic { Id = 1, PatientId = 1, DiagnosticTemplateId = 100, DateDiagnosed = DateTime.UtcNow, Notes = "Covid Test" };
            _context.Users.AddRange(doctor, patient);
            _context.PatientDiagnostics.Add(diagnostic);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "10", "Doctor");

            // Act
            var result = await _controller.GetDiagnostics(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var diagnostics = Assert.IsType<List<PatientDiagnosticDto>>(result.Value);
            Assert.Single(diagnostics);
        }

        /// <summary>
        /// Medical history retrieval returns 404 if not found.
        /// </summary>
        [Fact]
        public async Task GetMedicalHistory_ReturnsNotFound_WhenNoHistoryExists()
        {
            // Arrange
            var patient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            _context.Users.Add(patient);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetMedicalHistory(1) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// Posting a review returns a success message.
        /// </summary>
        [Fact]
        public void PostReview_ReturnsSuccessMessage()
        {
            // Arrange
            var patientId = 1;

            // Act
            var result = _controller.PostReview(patientId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Convert anonymous object to a dictionary using reflection
            var response = result.Value.GetType()
                .GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value)?.ToString());

            Assert.Contains("Message", response.Keys);
            Assert.Contains($"Review added for Patient {patientId} (mock response).", response.Values);
        }





        /// <summary>
        /// Helper method to set the fake HTTP context with user authentication
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
