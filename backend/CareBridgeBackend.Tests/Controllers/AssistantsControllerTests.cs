using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace CareBridgeBackend.Tests.Controllers
{
    public class AssistantsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AssistantsController _controller;

        public AssistantsControllerTests()
        {
            // Unique InMemoryDatabase for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new AssistantsController(_context);
        }

        /// <summary>
        /// Test: Successfully assign an assistant to a doctor.
        /// </summary>
        [Fact]
        public async Task AssignAssistant_Succeeds_WhenValid()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Doctor",
                LastName = "Smith",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };
            var assistant = new User
            {
                Id = 2,
                Role = UserRole.Assistant,
                FirstName = "Assistant",
                LastName = "Jones",
                Email = "assistant@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(doctor, assistant);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.AssignAssistant(2) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Check that the assignment exists in the database
            var assignmentExists = await _context.DoctorAssistants
                .AnyAsync(da => da.DoctorId == 1 && da.AssistantId == 2);

            Assert.True(assignmentExists);
        }

        /// <summary>
        /// Test: Fails when assigning a non-assistant user.
        /// </summary>
        [Fact]
        public async Task AssignAssistant_Fails_WhenUserIsNotAssistant()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Doctor",
                LastName = "Smith",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };
            var nonAssistant = new User
            {
                Id = 3,
                Role = UserRole.Patient, 
                FirstName = "NotAssistant",
                LastName = "Person",
                Email = "notassistant@example.com",
                Password = "hashed_password"
            };

            _context.Users.AddRange(doctor, nonAssistant);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.AssignAssistant(3) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);

            // Ensure that no assignment was created
            var assignmentExists = await _context.DoctorAssistants
                .AnyAsync(da => da.DoctorId == 1 && da.AssistantId == 3);

            Assert.False(assignmentExists);
        }

        /// <summary>
        /// Test: Should return the list of doctors an assistant is assigned to.
        /// </summary>
        [Fact]
        public async Task GetMyDoctors_ReturnsDoctors_WhenAssistantHasAssignments()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Doctor",
                LastName = "Smith",
                Specialization = "Cardiology",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };
            var assistant = new User
            {
                Id = 2,
                Role = UserRole.Assistant,
                FirstName = "Assistant",
                LastName = "Jones",
                Email = "assistant@example.com",
                Password = "hashed_password"
            };

            var assignment = new DoctorAssistant
            {
                DoctorId = 1,
                AssistantId = 2
            };

            _context.Users.AddRange(doctor, assistant);
            _context.DoctorAssistants.Add(assignment);
            await _context.SaveChangesAsync();

            // Ensure data was saved correctly
            var assignmentExists = await _context.DoctorAssistants
                .AnyAsync(da => da.DoctorId == 1 && da.AssistantId == 2);
            Assert.True(assignmentExists, "Assignment was not saved properly in the database.");

            SetFakeHttpContext(_controller, "2", "Assistant");

            // Act
            var result = await _controller.GetMyDoctors() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Log the actual returned value
            Console.WriteLine($"Returned Value: {result.Value}");

            // Ensure response is a list of doctors
            var doctors = ((IEnumerable<object>)result.Value).ToList(); // FIXED CASTING ISSUE
            Assert.NotNull(doctors);
            Assert.Single(doctors);
        }




        /// <summary>
        /// Test: Should remove an assistant from a doctor successfully.
        /// </summary>
        [Fact]
        public async Task RemoveAssistant_Succeeds_WhenAssignmentExists()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Doctor",
                LastName = "Smith",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };
            var assistant = new User
            {
                Id = 2,
                Role = UserRole.Assistant,
                FirstName = "Assistant",
                LastName = "Jones",
                Email = "assistant@example.com",
                Password = "hashed_password"
            };
            var assignment = new DoctorAssistant
            {
                DoctorId = 1,
                AssistantId = 2
            };

            _context.Users.AddRange(doctor, assistant);
            _context.DoctorAssistants.Add(assignment);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.RemoveAssistant(2) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Ensure that the assignment was removed
            var assignmentExists = await _context.DoctorAssistants
                .AnyAsync(da => da.DoctorId == 1 && da.AssistantId == 2);

            Assert.False(assignmentExists);
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
