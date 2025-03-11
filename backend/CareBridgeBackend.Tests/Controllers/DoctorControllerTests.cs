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
    public class DoctorsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DoctorsController _controller;

        public DoctorsControllerTests()
        {
            // Unique InMemoryDatabase for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new DoctorsController(_context);
        }

        /// <summary>
        /// ✅ Test: Get all doctors should return only doctors, not patients.
        /// </summary>
        [Fact]
        public async Task GetDoctors_ReturnsOnlyDoctors()
        {
            // Arrange
            var doctor1 = new User { Id = 1, Role = UserRole.Doctor, FirstName = "Alice", LastName = "Smith", Specialization = "Cardiology", LicenseNumber = "12345", Email = "alice@example.com", Password = "hashed_password" };
            var doctor2 = new User { Id = 2, Role = UserRole.Doctor, FirstName = "Bob", LastName = "Jones", Specialization = "Neurology", LicenseNumber = "67890", Email = "bob@example.com", Password = "hashed_password" };
            var patient = new User { Id = 3, Role = UserRole.Patient, FirstName = "Charlie", LastName = "Brown", Email = "charlie@example.com", Password = "hashed_password" };

            _context.Users.AddRange(doctor1, doctor2, patient);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDoctors() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Use IEnumerable<object> instead of List<dynamic>
            var doctors = result.Value as IEnumerable<object>;

            Assert.NotNull(doctors);
            Assert.Equal(2, doctors.Count());
            Assert.All(doctors, d => Assert.NotNull(d.GetType().GetProperty("Specialization"))); // Ensure specialization exists
        }


        /// <summary>
        /// ✅ Test: Get a specific doctor by ID should return correct details.
        /// </summary>
        [Fact]
        public async Task GetDoctor_ReturnsDoctor_WhenExists()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Alice",
                LastName = "Smith",
                Specialization = "Cardiology",
                LicenseNumber = "12345",
                Email = "alice@example.com",
                Password = "hashed_password"
            };

            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDoctor(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Extract values using reflection
            var returnedDoctor = result.Value;
            Assert.NotNull(returnedDoctor);

            var type = returnedDoctor.GetType();

            var id = (int)type.GetProperty("Id").GetValue(returnedDoctor);
            var firstName = (string)type.GetProperty("FirstName").GetValue(returnedDoctor);

            Assert.Equal(1, id);
            Assert.Equal("Alice", firstName);
        }






        /// <summary>
        /// ✅ Test: Get doctor by ID should return 404 if doctor does not exist.
        /// </summary>
        [Fact]
        public async Task GetDoctor_ReturnsNotFound_WhenDoctorDoesNotExist()
        {
            // Act
            var result = await _controller.GetDoctor(99) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Get doctor's appointments should return correct list.
        /// </summary>
        [Fact]
        public async Task GetMyAppointments_ReturnsAppointments_WhenDoctorHasAppointments()
        {
            // Arrange
            var doctor = new User { Id = 1, Role = UserRole.Doctor, FirstName = "Dr. Alice", LastName = "Smith", Email = "doctor@example.com", Password = "hashed_password" };
            var patient = new User { Id = 2, Role = UserRole.Patient, FirstName = "John", LastName = "Doe", Email = "patient@example.com", Password = "hashed_password" };

            _context.Users.AddRange(doctor, patient);
            await _context.SaveChangesAsync();

            var appointment = new Appointment { Id = 1, DoctorId = 1, PatientId = 2, AppointmentDate = DateTime.UtcNow, Notes = "Follow-up" };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMyAppointments() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var appointments = result.Value as List<AppointmentDto>;
            Assert.NotNull(appointments);
            Assert.Single(appointments);
            Assert.Equal("Follow-up", appointments[0].Notes);
        }

        /// <summary>
        /// ✅ Test: Get doctor's appointments should return empty if none exist.
        /// </summary>
        [Fact]
        public async Task GetMyAppointments_ReturnsEmpty_WhenNoAppointments()
        {
            // Arrange
            var doctor = new User { Id = 1, Role = UserRole.Doctor, FirstName = "Dr. Alice", LastName = "Smith", Email = "doctor@example.com", Password = "hashed_password" };

            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMyAppointments() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var appointments = result.Value as List<AppointmentDto>;
            Assert.Empty(appointments);
        }

        /// <summary>
        /// ✅ Test: Get doctor's patients should return list of unique patients.
        /// </summary>
        [Fact]
        public async Task GetMyPatients_ReturnsPatients_WhenDoctorHasPatients()
        {
            // Arrange
            var doctor = new User { Id = 1, Role = UserRole.Doctor, FirstName = "Dr. Alice", LastName = "Smith", Email = "doctor@example.com", Password = "hashed_password" };
            var patient = new User { Id = 2, Role = UserRole.Patient, FirstName = "John", LastName = "Doe", Email = "johndoe@example.com", Password = "hashed_password" };

            _context.Users.AddRange(doctor, patient);
            await _context.SaveChangesAsync();

            // Ensure the Appointment is saved and tracked correctly
            var appointment = new Appointment { Id = 1, DoctorId = 1, PatientId = 2, AppointmentDate = DateTime.UtcNow, Notes = "Follow-up" };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMyPatients() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Cast as IEnumerable<object> to handle anonymous types properly
            var patients = result.Value as IEnumerable<object>;

            Assert.NotNull(patients);   
            Assert.NotEmpty(patients); 
        }




        /// <summary>
        /// ✅ Test: Get doctor's patients should return empty list if no patients exist.
        /// </summary>
        [Fact]
        public async Task GetMyPatients_ReturnsEmpty_WhenNoPatients()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. Alice",
                LastName = "Smith",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };

            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMyPatients() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Ensure the response is an empty list, not null
            var patients = result.Value as IEnumerable<object>;
            Assert.NotNull(patients);  
            Assert.Empty(patients);    
        }


        #region Helpers
        private void SetFakeHttpContext(ControllerBase controller, string userId, string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) } };
        }

        public void Dispose() => _context.Dispose();
        #endregion
    }
}
