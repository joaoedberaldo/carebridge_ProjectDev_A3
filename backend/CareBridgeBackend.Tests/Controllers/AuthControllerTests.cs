using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using CareBridgeBackend.Helpers;
using CareBridgeBackend.Models;
using System.Collections.Generic;
using System.Security.Claims;  
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;

namespace CareBridgeBackend.Tests.Controllers
{
    public class AuthControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            // Unique in-memory database instance for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            var jwtHelper = new JwtHelper(
                "SuperSecretKeyForJwtTestingMustBe32Char", // ✅ 32+ characters
                "CareBridgeIssuer",
                "CareBridgeAudience"
            );
            _controller = new AuthController(_context, jwtHelper);
        }

        /// <summary>
        /// Test: Should register a new user successfully.
        /// </summary>
        [Fact]
        public async Task Register_Succeeds_WhenValidData()
        {
            // Arrange
            var userDto = new UserRegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Password = "StrongPassword123!",
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1990, 1, 1),
                PhoneNumber = "123-456-7890"
            };

            // Act
            var result = await _controller.Register(userDto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);

            // Ensure the user was actually added to the database
            var userExists = await _context.Users.AnyAsync(u => u.Email == userDto.Email);
            Assert.True(userExists);
        }

        /// <summary>
        /// Test: Should fail to register when email already exists.
        /// </summary>
        [Fact]
        public async Task Register_Fails_WhenEmailAlreadyExists()
        {
            // Arrange
            var existingUser = new User
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "janedoe@example.com",
                Password = PasswordHelper.HashPassword("SecurePassword!"),
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1992, 2, 2)
            };

            _context.Users.Add(existingUser);
            await _context.SaveChangesAsync();

            var userDto = new UserRegisterDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "janedoe@example.com", // Same email as existing user
                Password = "StrongPassword123!",
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            // Act
            var result = await _controller.Register(userDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        /// <summary>
        /// Test: Should login successfully and return a JWT token.
        /// </summary>
        [Fact]
        public async Task Login_Succeeds_WhenCredentialsAreValid()
        {
            // Arrange
            var password = "StrongPassword123!";
            var hashedPassword = PasswordHelper.HashPassword(password);
            var testUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Password = hashedPassword,
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            var loginDto = new UserLoginDto
            {
                Email = "johndoe@example.com",
                Password = password
            };

            // Act
            var result = await _controller.Login(loginDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.Contains("Token", result.Value.ToString());
        }

        /// <summary>
        /// Test: Should return Unauthorized when credentials are incorrect.
        /// </summary>
        [Fact]
        public async Task Login_Fails_WhenInvalidCredentials()
        {
            // Arrange
            var testUser = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Password = PasswordHelper.HashPassword("CorrectPassword"),
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1990, 1, 1)
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            var loginDto = new UserLoginDto
            {
                Email = "johndoe@example.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await _controller.Login(loginDto) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        /// <summary>
        /// Test: Should get the logged-in user's profile.
        /// </summary>
        [Fact]
        public async Task GetMyProfile_ReturnsUserDetails_WhenAuthenticated()
        {
            // Arrange
            var testUser = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "johndoe@example.com",
                Password = PasswordHelper.HashPassword("CorrectPassword"),
                Role = UserRole.Patient,
                DateOfBirth = new DateTime(1990, 1, 1),
                PhoneNumber = "123-456-7890"
            };

            _context.Users.Add(testUser);
            await _context.SaveChangesAsync();

            // Simulate authenticated request
            SetFakeHttpContext(_controller, "1", "Patient");

            // Act
            var result = await _controller.GetMyProfile() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Convert the returned object to a known type
            var userResponse = result.Value.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            // Assertions
            Assert.Equal(1, userResponse["Id"]);
            Assert.Equal("John", userResponse["FirstName"]);
            Assert.Equal("Doe", userResponse["LastName"]);
            Assert.Equal("johndoe@example.com", userResponse["Email"]);
            Assert.Equal(UserRole.Patient, userResponse["Role"]);
            Assert.Equal("123-456-7890", userResponse["PhoneNumber"]);
        }


        #region Helpers

        /// <summary>
        /// Helper method to simulate authentication context.
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
