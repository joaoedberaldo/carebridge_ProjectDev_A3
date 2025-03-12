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
    public class PrescriptionsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly PrescriptionsController _controller;

        public PrescriptionsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new PrescriptionsController(_context);
        }

        /// <summary>
        /// Doctors can create prescriptions successfully.
        /// </summary>
        [Fact]
        public async Task CreatePrescription_Succeeds_WhenValid()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            _context.Users.AddRange(doctor, patient);
            await _context.SaveChangesAsync();

            var prescriptionDto = new PrescriptionCreateDto
            {
                PatientId = 2,
                Description = "Take medicine twice daily."
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.CreatePrescription(prescriptionDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);

            var response = result.Value.GetType()
                .GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value)?.ToString());

            Assert.Contains("Message", response.Keys);
            Assert.Contains("Prescription created successfully.", response.Values);
        }

        /// <summary>
        /// Patients cannot create prescriptions.
        /// </summary>
        [Fact]
        public async Task CreatePrescription_ReturnsUnauthorized_WhenPatientTries()
        {
            // Arrange
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            _context.Users.Add(patient);
            await _context.SaveChangesAsync();

            var prescriptionDto = new PrescriptionCreateDto
            {
                PatientId = 2,
                Description = "Unauthorized attempt"
            };

            SetFakeHttpContext(_controller, "2", "Patient");

            // Act
            var result = await _controller.CreatePrescription(prescriptionDto);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ForbidResult>(result); // Ensure Forbid is returned
        }


        /// <summary>
        /// Patients can retrieve their own prescriptions.
        /// </summary>
        [Fact]
        public async Task GetPrescriptionsForPatient_ReturnsPrescriptions_WhenPatientOwnsThem()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var prescription = new Prescription { Id = 1, PatientId = 2, DoctorId = 1, Description = "Painkillers", Date = DateTime.UtcNow, Status = PrescriptionStatus.Active };

            _context.Users.AddRange(doctor, patient);
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "2", "Patient");

            // Act
            var result = await _controller.GetPrescriptionsForPatient(2) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var prescriptions = Assert.IsType<List<PrescriptionDto>>(result.Value);
            Assert.Single(prescriptions);
        }

        /// <summary>
        /// Patients cannot retrieve prescriptions of other patients.
        /// </summary>
        [Fact]
        public async Task GetPrescriptionsForPatient_ReturnsUnauthorized_WhenAccessingOthers()
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
            var result = await _controller.GetPrescriptionsForPatient(2) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        /// <summary>
        /// Doctors can retrieve a patient's prescriptions.
        /// </summary>
        [Fact]
        public async Task GetPrescriptionsForPatient_ReturnsPrescriptions_WhenDoctorRequests()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var patient = new User
            {
                Id = 2,
                Role = UserRole.Patient,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "hashed_password"
            };
            var prescription = new Prescription { Id = 1, PatientId = 2, DoctorId = 1, Description = "Flu Medication", Date = DateTime.UtcNow, Status = PrescriptionStatus.Active };

            _context.Users.AddRange(doctor, patient);
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetPrescriptionsForPatient(2) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var prescriptions = Assert.IsType<List<PrescriptionDto>>(result.Value);
            Assert.Single(prescriptions);
        }

        /// <summary>
        /// Doctors can delete their own prescriptions.
        /// </summary>
        [Fact]
        public async Task DeletePrescription_Succeeds_WhenDoctorOwnsIt()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var prescription = new Prescription { Id = 1, PatientId = 2, DoctorId = 1, Description = "Test Prescription", Status = PrescriptionStatus.Active };

            _context.Users.Add(doctor);
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.DeletePrescription(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.DoesNotContain(_context.Prescriptions, p => p.Id == 1);
        }

        /// <summary>
        /// Doctors cannot delete another doctor's prescriptions.
        /// </summary>
        [Fact]
        public async Task DeletePrescription_ReturnsUnauthorized_WhenDoctorDoesNotOwnIt()
        {
            // Arrange
            var doctor1 = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Smith",
                LastName = "harvard",
                Email = "dr.harvard@example.com",
                Password = "hashed_password"
            };
            var doctor2 = new User
            {
                Id = 2,
                Role = UserRole.Doctor,
                FirstName = "Dr. Emily",
                LastName = "Johnson",
                Email = "dr.johnson@example.com",
                Password = "hashed_password"
            };
            var prescription = new Prescription { Id = 1, PatientId = 3, DoctorId = 2, Description = "Antibiotics", Status = PrescriptionStatus.Active };

            _context.Users.AddRange(doctor1, doctor2);
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.DeletePrescription(1) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
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
