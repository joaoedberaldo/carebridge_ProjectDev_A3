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
    public class DoctorSchedulesControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly DoctorSchedulesController _controller;

        public DoctorSchedulesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new DoctorSchedulesController(_context);
        }

        /// <summary>
        /// Test: Returns doctor's schedules when they exist.
        /// </summary>
        [Fact]
        public async Task GetMySchedules_ReturnsSchedules_WhenDoctorHasSchedules()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. John",
                LastName = "Doe",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };

            var schedule = new DoctorSchedule { DoctorId = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Description = "Consultation" };
            _context.Users.Add(doctor);
            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.GetMySchedules() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var schedules = result.Value as List<DoctorSchedule>;
            Assert.NotNull(schedules);
            Assert.Single(schedules);
        }

        /// <summary>
        /// Test: Creating a schedule succeeds when valid.
        /// </summary>
        [Fact]
        public async Task CreateSchedule_Succeeds_WhenValid()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. John",
                LastName = "Doe",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };

            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            var scheduleDto = new DoctorScheduleCreateDto
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                Description = "New Schedule"
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.CreateSchedule(scheduleDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.True(await _context.DoctorSchedules.AnyAsync(s => s.Description == "New Schedule"));
        }

        /// <summary>
        /// Test: Updating a schedule succeeds when owned by doctor.
        /// </summary>
        [Fact]
        public async Task UpdateSchedule_Succeeds_WhenDoctorOwnsSchedule()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. John",
                LastName = "Doe",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };

            var schedule = new DoctorSchedule { Id = 1, DoctorId = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Description = "Old" };
            _context.Users.Add(doctor);
            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            var updateDto = new DoctorScheduleCreateDto
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(2),
                Description = "Updated"
            };

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.UpdateSchedule(1, updateDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            var updatedSchedule = await _context.DoctorSchedules.FindAsync(1);
            Assert.Equal("Updated", updatedSchedule.Description);
        }

        /// <summary>
        /// Test: Deleting a schedule succeeds when owned by doctor.
        /// </summary>
        [Fact]
        public async Task DeleteSchedule_Succeeds_WhenDoctorOwnsSchedule()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Dr. John",
                LastName = "Doe",
                Email = "doctor@example.com",
                Password = "hashed_password"
            };

            var schedule = new DoctorSchedule { Id = 1, DoctorId = 1, StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1), Description = "Delete Me" };
            _context.Users.Add(doctor);
            _context.DoctorSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            SetFakeHttpContext(_controller, "1", "Doctor");

            // Act
            var result = await _controller.DeleteSchedule(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.False(await _context.DoctorSchedules.AnyAsync(s => s.Id == 1));
        }

        #region Helpers
        private void SetFakeHttpContext(ControllerBase controller, string userId, string role)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = claimsPrincipal } };
        }
        public void Dispose() { _context.Dispose(); }
        #endregion
    }
}
