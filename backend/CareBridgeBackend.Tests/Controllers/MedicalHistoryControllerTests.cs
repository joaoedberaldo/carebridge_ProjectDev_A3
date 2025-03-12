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
    public class MedicalHistoryControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly MedicalHistoryController _controller;

        public MedicalHistoryControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new MedicalHistoryController(_context);
        }

        /// <summary>
        /// ✅ Test: Adding a medical history should succeed for valid patients.
        /// </summary>
        [Fact]
        public async Task AddMedicalHistory_CreatesNewHistory_WhenValid()
        {
            // Arrange
            var patient = new User { Id = 1, Role = UserRole.Patient, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "hashed_password" };
            _context.Users.Add(patient);
            await _context.SaveChangesAsync();

            var dto = new MedicalHistoryCreateDto { PatientId = 1 };

            // Act
            var result = await _controller.AddMedicalHistory(dto) as OkObjectResult;


            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Extract `MedicalHistoryId` using Dictionary
            var responseDict = result.Value?.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.NotNull(responseDict);
            Assert.True(responseDict.ContainsKey("MedicalHistoryId"));
            Assert.True((int)responseDict["MedicalHistoryId"] > 0);

            // Ensure it's actually saved
            var savedHistory = await _context.MedicalHistories.FindAsync(responseDict["MedicalHistoryId"]);
            Assert.NotNull(savedHistory);
        }




        /// <summary>
        /// ✅ Test: Adding medical history should fail if the patient does not exist.
        /// </summary>
        [Fact]
        public async Task AddMedicalHistory_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            // Arrange
            var dto = new MedicalHistoryCreateDto { PatientId = 99 }; // Non-existent patient

            // Act
            var result = await _controller.AddMedicalHistory(dto) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Verify medical history should return ID if history exists.
        /// </summary>
        [Fact]
        public async Task VerifyMedicalHistory_ReturnsId_WhenExists()
        {
            // Arrange
            var patient = new User { Id = 1, Role = UserRole.Patient, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com", Password = "hashed_password" };
            _context.Users.Add(patient);
            await _context.SaveChangesAsync();

            var history = new MedicalHistory { Id = 1, PatientId = 1 };
            _context.MedicalHistories.Add(history);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.VerifyMedicalHistory(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Extract `MedicalHistoryId` using Dictionary
            var responseDict = result.Value?.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.NotNull(responseDict);
            Assert.True(responseDict.ContainsKey("MedicalHistoryId"));
            Assert.Equal(1, (int)responseDict["MedicalHistoryId"]);
        }



        /// <summary>
        /// ✅ Test: Verify medical history should return zero if none exist.
        /// </summary>
        [Fact]
        public async Task VerifyMedicalHistory_ReturnsZero_WhenNotExists()
        {
            // Act
            var result = await _controller.VerifyMedicalHistory(99) as OkObjectResult;

            // 🔹 Debugging Output
            Console.WriteLine($"Returned Value: {result?.Value}");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Extract `MedicalHistoryId` dynamically
            var responseDict = result.Value?.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.NotNull(responseDict);
            Assert.True(responseDict.ContainsKey("MedicalHistoryId"));
            Assert.Equal(0, (int)responseDict["MedicalHistoryId"]);
        }



        /// <summary>
        /// ✅ Test: Getting a medical history should return details if authorized.
        /// </summary>
        [Fact]
        public async Task GetMedicalHistory_ReturnsHistory_WhenAuthorized()
        {
            // Arrange
            var patient = new User { Id = 1, Role = UserRole.Patient, FirstName = "John", LastName = "Doe", Email = "john@example.com", Password = "hashed_password" };
            _context.Users.Add(patient);
            await _context.SaveChangesAsync();

            var history = new MedicalHistory { Id = 1, PatientId = 1 };
            _context.MedicalHistories.Add(history);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient"); // Authenticated as the same patient

            // Act
            var result = await _controller.GetMedicalHistory(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value as MedicalHistoryDto;
            Assert.NotNull(response);
            Assert.Equal(1, response.Id);
            Assert.Equal(1, response.PatientId);
        }

        /// <summary>
        /// ✅ Test: A patient should not access another patient's medical history.
        /// </summary>
        [Fact]
        public async Task GetMedicalHistory_ReturnsUnauthorized_WhenPatientAccessesOthers()
        {
            // Arrange
            var history = new MedicalHistory { Id = 1, PatientId = 1 };
            _context.MedicalHistories.Add(history);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "2", "Patient"); // Different patient

            // Act
            var result = await _controller.GetMedicalHistory(1) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Getting a medical history should return 404 if not found.
        /// </summary>
        [Fact]
        public async Task GetMedicalHistory_ReturnsNotFound_WhenNoHistory()
        {
            // Arrange
            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMedicalHistory(1) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: A doctor should be able to delete a medical history.
        /// </summary>
        [Fact]
        public async Task DeleteMedicalHistory_DeletesHistory_WhenDoctor()
        {
            // Arrange
            var history = new MedicalHistory { Id = 1, PatientId = 1 };
            _context.MedicalHistories.Add(history);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "2", "Doctor"); // Simulating a doctor user

            // Act
            var result = await _controller.DeleteMedicalHistory(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // 🔹 Fix: Extract "Message" dynamically
            var responseDict = result.Value?.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.NotNull(responseDict);
            Assert.True(responseDict.ContainsKey("Message"));
            Assert.Equal("Medical history deleted successfully.", (string)responseDict["Message"]);

            // Ensure it's actually deleted
            var deleted = await _context.MedicalHistories.FindAsync(1);
            Assert.Null(deleted);
        }



        /// <summary>
        /// ✅ Test: Deleting a non-existent medical history should return 404.
        /// </summary>
        [Fact]
        public async Task DeleteMedicalHistory_ReturnsNotFound_WhenNoHistoryExists()
        {
            // Arrange
            SetFakeHttpContext(_controller, "2", "Doctor");

            // Act
            var result = await _controller.DeleteMedicalHistory(99) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        #region Helpers
        private void SetFakeHttpContext(ControllerBase controller, string userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, role)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        public void Dispose() => _context.Dispose();
        #endregion
    }
}
