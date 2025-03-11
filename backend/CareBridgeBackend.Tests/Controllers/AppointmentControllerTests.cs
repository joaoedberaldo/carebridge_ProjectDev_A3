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

namespace CareBridgeBackend.Tests.Controllers
{
    public class AppointmentControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentsController _controller;

        public AppointmentControllerTests()
        {
            // Ensure a unique DB instance for each test to avoid data conflicts
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new AppointmentsController(_context);
        }

        #region Create Appointment
        /// <summary>
        /// Test: Should return BadRequest when the input DTO is null.
        /// </summary>
        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAppointment(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Test: Should return Unauthorized when a patient tries to book an appointment for another patient.
        /// </summary>
        [Fact]
        public async Task CreateAppointment_ReturnsUnauthorized_WhenPatientBooksForAnotherPatient()
        {
            // Arrange
            var testPatient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                Email = "patient@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "hashed_password"
            };
            _context.Users.Add(testPatient);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentUpdateDto
            {
                PatientId = 2, // Wrong patient ID
                DoctorId = 3,
                AppointmentDate = DateTime.UtcNow
            };

            SetFakeHttpContext(_controller, "1", "Patient"); // Simulate logged-in patient (ID = 1)

            // Act
            var result = await _controller.CreateAppointment(appointmentDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        /// <summary>
        /// Test: Should return BadRequest when the doctor does not exist.
        /// </summary>
        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_WhenDoctorDoesNotExist()
        {
            // Arrange
            var testPatient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                Email = "patient@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "hashed_password"
            };
            _context.Users.Add(testPatient);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentUpdateDto
            {
                PatientId = 1,
                DoctorId = 99, // Non-existent doctor ID
                AppointmentDate = DateTime.UtcNow,
                Notes = "Test Notes"
            };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.CreateAppointment(appointmentDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        /// <summary>
        /// Test: Should create a new MedicalHistory for a patient if none exists.
        /// </summary>
        [Fact]
        public async Task CreateAppointment_CreatesMedicalHistory_IfNoneExists()
        {
            // Arrange
            var testPatient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                Email = "patient@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "hashed_password"
            };
            var testDoctor = new User
            {
                Id = 2,
                Role = UserRole.Doctor,
                Email = "doctor@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "hashed_password"
            };

            _context.Users.AddRange(testPatient, testDoctor);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentUpdateDto
            {
                PatientId = 1,
                DoctorId = 2,
                AppointmentDate = DateTime.UtcNow,
                Notes = "Test Notes"
            };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.CreateAppointment(appointmentDto);

            // Assert
            var medicalHistoryExists = await _context.MedicalHistories.AnyAsync(mh => mh.PatientId == 1);
            Assert.True(medicalHistoryExists);
        }

        /// <summary>
        /// Test: Should successfully create an appointment.
        /// </summary>
        [Fact]
        public async Task CreateAppointment_ReturnsCreated_WhenSuccessful()
        {
            // Arrange
            var testPatient = new User
            {
                Id = 1,
                Role = UserRole.Patient,
                Email = "patient@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "hashed_password"
            };
            var testDoctor = new User
            {
                Id = 2,
                Role = UserRole.Doctor,
                Email = "doctor@example.com",
                FirstName = "Jane",
                LastName = "Smith",
                Password = "hashed_password"
            };

            _context.Users.AddRange(testPatient, testDoctor);
            await _context.SaveChangesAsync();

            var appointmentDto = new AppointmentUpdateDto
            {
                PatientId = 1,
                DoctorId = 2,
                AppointmentDate = DateTime.UtcNow,
                Notes = "Test Notes"
            };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.CreateAppointment(appointmentDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(AppointmentsController.GetAppointment), result.ActionName); // Verify correct action
        }
        #endregion

        #region Update Appointment

        /// <summary>
        /// Test: Should return NotFound when updating a non-existent appointment.
        /// </summary>
        [Fact]
        public async Task UpdateAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            var dto = new AppointmentUpdateDto { Notes = "Updated Notes" };
            SetFakeHttpContext(_controller, "1", "Doctor");

            var result = await _controller.UpdateAppointment(99, dto);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Test: Should return Unauthorized if a patient tries to update an appointment.
        /// </summary>
        [Fact]
        public async Task UpdateAppointment_ReturnsUnauthorized_WhenPatientTriesToUpdate()
        {
            var testAppointment = await CreateTestAppointment();
            var dto = new AppointmentUpdateDto { Notes = "Updated Notes" };

            SetFakeHttpContext(_controller, testAppointment.PatientId.ToString(), "Patient");
            var result = await _controller.UpdateAppointment(testAppointment.Id, dto);

            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        /// <summary>
        /// Test: Should successfully update an appointment when a doctor updates it.
        /// </summary>
        [Fact]
        public async Task UpdateAppointment_ReturnsOk_WhenDoctorUpdatesSuccessfully()
        {
            var testAppointment = await CreateTestAppointment();
            var dto = new AppointmentUpdateDto { Notes = "Updated Notes" };

            SetFakeHttpContext(_controller, testAppointment.DoctorId.ToString(), "Doctor");
            var result = await _controller.UpdateAppointment(testAppointment.Id, dto);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Delete Appointment
        /// <summary>
        /// Test: Should return NotFound when deleting a non-existent appointment.
        /// </summary>
        [Fact]
        public async Task DeleteAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            SetFakeHttpContext(_controller, "1", "Doctor");
            var result = await _controller.DeleteAppointment(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Test: Should successfully delete an appointment when a doctor deletes it.
        /// </summary>
        [Fact]
        public async Task DeleteAppointment_ReturnsOk_WhenDoctorDeletesSuccessfully()
        {
            var testAppointment = await CreateTestAppointment();
            SetFakeHttpContext(_controller, testAppointment.DoctorId.ToString(), "Doctor");

            var result = await _controller.DeleteAppointment(testAppointment.Id);
            Assert.IsType<OkObjectResult>(result);
        }
        #endregion

        #region Get Appointment
        /// <summary>
        /// Test: Should return NotFound when retrieving a non-existent appointment.
        /// </summary>
        [Fact]
        public async Task GetAppointment_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            var result = await _controller.GetAppointment(99);
            Assert.IsType<NotFoundObjectResult>(result);
        }

        /// <summary>
        /// Test: Should return correct appointment details.
        /// </summary>
        [Fact]
        public async Task GetAppointment_ReturnsCorrectDetails()
        {
            var testAppointment = await CreateTestAppointment();
            var result = await _controller.GetAppointment(testAppointment.Id) as OkObjectResult;

            Assert.NotNull(result);
        }
        #endregion

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
        /// Helper method to create a test appointment in the database.
        /// </summary>
        private async Task<Appointment> CreateTestAppointment()
        {
            var patient = new User { Id = 1, Role = UserRole.Patient, Email = "patient@example.com", FirstName = "John", LastName = "Doe", Password = "hashed_password" };
            var doctor = new User { Id = 2, Role = UserRole.Doctor, Email = "doctor@example.com", FirstName = "Jane", LastName = "Smith", Password = "hashed_password" };
            var appointment = new Appointment { Id = 10, PatientId = 1, DoctorId = 2, AppointmentDate = DateTime.UtcNow, Notes = "Initial Notes" };

            _context.Users.AddRange(patient, doctor);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;
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
