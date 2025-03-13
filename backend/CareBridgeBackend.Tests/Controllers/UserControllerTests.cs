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
    public class UsersControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new UsersController(_context);
        }

        /// <summary>
        /// Users can retrieve their own details successfully.
        /// </summary>
        [Fact]
        public async Task GetUser_ReturnsUserDetails_WhenUserExists()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "Alice",
                LastName = "Doe",
                Email = "alice@example.com",
                Role = UserRole.Patient,
                PhoneNumber = "123-456-7890",
                Password = "hashed_password"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetUser(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var response = result.Value.GetType()
                .GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.Equal(1, response["Id"]);
            Assert.Equal("Alice", response["FirstName"]);
            Assert.Equal("Doe", response["LastName"]);
            Assert.Equal("alice@example.com", response["Email"]);
            Assert.Equal(UserRole.Patient, response["Role"]);
            Assert.Equal("123-456-7890", response["PhoneNumber"]);
        }

        /// <summary>
        /// Trying to retrieve a non-existent user returns NotFound.
        /// </summary>
        [Fact]
        public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetUser(999) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("User not found", result.Value.ToString());
        }

        /// <summary>
        /// Users can update their own profile.
        /// </summary>
        [Fact]
        public async Task UpdateUser_Succeeds_WhenUserUpdatesOwnProfile()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Role = UserRole.Patient,
                PhoneNumber = "111-222-3333",
                Password = "hashed_password"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new UserUpdateDto
            {
                FirstName = "Johnny",
                LastName = "Updated",
                PhoneNumber = "999-888-7777"
            };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.UpdateUser(1, updateDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("User updated successfully", result.Value.ToString());

            // Verify the update
            var updatedUser = await _context.Users.FindAsync(1);
            Assert.Equal("Johnny", updatedUser.FirstName);
            Assert.Equal("Updated", updatedUser.LastName);
            Assert.Equal("999-888-7777", updatedUser.PhoneNumber);
        }

        /// <summary>
        /// Users cannot update other users' profiles unless they are a doctor.
        /// </summary>
        [Fact]
        public async Task UpdateUser_ReturnsUnauthorized_WhenUserUpdatesOthersProfile()
        {
            // Arrange
            var user1 = new User
            {
                Id = 1,
                FirstName = "User1",
                LastName = "LastName",
                Email = "john@example.com",
                Role = UserRole.Patient,
                PhoneNumber = "111-222-3333",
                Password = "hashed_password"
            };
            var user2 = new User
            {
                Id = 2,
                FirstName = "User2",
                LastName = "LastName",
                Email = "john@example.com",
                Role = UserRole.Patient,
                PhoneNumber = "111-222-3333",
                Password = "hashed_password"
            };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            var updateDto = new UserUpdateDto { FirstName = "Hacker" };

            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.UpdateUser(2, updateDto) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
            Assert.Contains("You can only update your own profile", result.Value.ToString());
        }

        /// <summary>
        /// Doctors can update other users' profiles.
        /// </summary>
        [Fact]
        public async Task UpdateUser_Succeeds_WhenDoctorUpdatesUserProfile()
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

            var updateDto = new UserUpdateDto { FirstName = "UpdatedJane" };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.UpdateUser(2, updateDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("User updated successfully", result.Value.ToString());

            var updatedUser = await _context.Users.FindAsync(2);
            Assert.Equal("UpdatedJane", updatedUser.FirstName);
        }

        /// <summary>
        /// Assigning an office to a doctor succeeds.
        /// </summary>
        [Fact]
        public async Task AssignOffice_Succeeds_WhenValid()
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
            var office = new Office
            {
                Id = 100,
                Name = "Health Clinic",
                Address = "123 Main St",
                City = "Toronto",
                State = "ON",
                ZipCode = "A1B2C3"
            };


            _context.Users.Add(doctor);
            _context.Offices.Add(office);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.AssignOffice(1, 100) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Doctor 1 assigned to Office 100", result.Value.ToString());

            var updatedDoctor = await _context.Users.FindAsync(1);
            Assert.Equal(100, updatedDoctor.OfficeId);
        }

        /// <summary>
        /// Assigning an office fails when the doctor ID is invalid.
        /// </summary>
        [Fact]
        public async Task AssignOffice_ReturnsBadRequest_WhenDoctorIsInvalid()
        {
            // Arrange
            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.AssignOffice(999, 100) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Invalid doctor ID", result.Value.ToString());
        }

        /// <summary>
        /// Assigning an office fails when the office does not exist.
        /// </summary>
        [Fact]
        public async Task AssignOffice_ReturnsNotFound_WhenOfficeDoesNotExist()
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
            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.AssignOffice(1, 999) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
            Assert.Contains("Office not found", result.Value.ToString());
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

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
