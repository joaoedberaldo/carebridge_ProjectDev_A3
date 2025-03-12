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
    public class OfficesControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly OfficesController _controller;

        public OfficesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _controller = new OfficesController(_context);
        }

        /// <summary>
        /// ✅ Test: Creating an office should return 201 Created and persist the data.
        /// </summary>
        [Fact]
        public async Task CreateOffice_CreatesNewOffice_WhenValid()
        {
            // Arrange
            var dto = new OfficeCreateDto
            {
                Name = "Downtown Clinic",
                Address = "123 Main St",
                City = "Toronto",
                State = "ON",
                ZipCode = "A1B2C3"
            };

            // Act
            var result = await _controller.CreateOffice(dto) as CreatedAtActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(201, result.StatusCode);

            // Extract properties dynamically
            var responseDict = result.Value?.GetType().GetProperties()
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(result.Value));

            Assert.NotNull(responseDict);
            Assert.True(responseDict.ContainsKey("OfficeId"));
            Assert.True((int)responseDict["OfficeId"] > 0);

            // Ensure persistence
            var savedOffice = await _context.Offices.FindAsync(responseDict["OfficeId"]);
            Assert.NotNull(savedOffice);
            Assert.Equal(dto.Name, savedOffice.Name);
        }

        /// <summary>
        /// ✅ Test: Retrieving an existing office should return the correct office details.
        /// </summary>
        [Fact]
        public async Task GetOffice_ReturnsOffice_WhenExists()
        {
            // Arrange
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice@example.com",
                Password = "hashed_password"
            };

            var office = new Office
            {
                Id = 1,
                Name = "Downtown Clinic",
                Address = "123 Main St",
                City = "Toronto",
                State = "ON",
                ZipCode = "A1B2C3",
                Doctors = new List<User> { doctor }
            };

            _context.Offices.Add(office);
            _context.Users.Add(doctor); 
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetOffice(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var officeDto = result.Value as OfficeDto;
            Assert.NotNull(officeDto);
            Assert.Equal(1, officeDto.Id);
            Assert.Single(officeDto.DoctorIds);
            Assert.Contains(1, officeDto.DoctorIds);
        }


        /// <summary>
        /// ✅ Test: Retrieving a non-existing office should return 404 Not Found.
        /// </summary>
        [Fact]
        public async Task GetOffice_ReturnsNotFound_WhenOfficeDoesNotExist()
        {
            // Act
            var result = await _controller.GetOffice(99) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Updating an existing office should apply the changes and return success.
        /// </summary>
        [Fact]
        public async Task UpdateOffice_UpdatesOffice_WhenExists()
        {
            // ✅ FIX: Ensure all required fields are set
            var office = new Office
            {
                Id = 1,
                Name = "Initial Clinic",
                Address = "101 Bay St",
                City = "Vancouver",
                State = "BC",
                ZipCode = "V6C3P3"
            };
            _context.Offices.Add(office);
            await _context.SaveChangesAsync();

            var dto = new OfficeCreateDto
            {
                Name = "Updated Clinic",
                Address = "789 Maple St",
                City = "Montreal",
                State = "QC",
                ZipCode = "H2K2T3"
            };

            // Act
            var result = await _controller.UpdateOffice(1, dto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Ensure persistence
            var updatedOffice = await _context.Offices.FindAsync(1);
            Assert.NotNull(updatedOffice);
            Assert.Equal(dto.Name, updatedOffice.Name);
        }

        /// <summary>
        /// ✅ Test: Updating a non-existing office should return 404 Not Found.
        /// </summary>
        [Fact]
        public async Task UpdateOffice_ReturnsNotFound_WhenOfficeDoesNotExist()
        {
            // Act
            var result = await _controller.UpdateOffice(99, new OfficeCreateDto { Name = "New Clinic" }) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Deleting an existing office should remove it from the database.
        /// </summary>
        [Fact]
        public async Task DeleteOffice_DeletesOffice_WhenExists()
        {
            // ✅ FIX: Ensure all required fields are set
            var office = new Office
            {
                Id = 1,
                Name = "Clinic A",
                Address = "456 Elm St",
                City = "Toronto",
                State = "ON",
                ZipCode = "M5H2N2"
            };
            _context.Offices.Add(office);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteOffice(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Ensure deletion
            var deletedOffice = await _context.Offices.FindAsync(1);
            Assert.Null(deletedOffice);
        }

        /// <summary>
        /// ✅ Test: Deleting a non-existing office should return 404 Not Found.
        /// </summary>
        [Fact]
        public async Task DeleteOffice_ReturnsNotFound_WhenOfficeDoesNotExist()
        {
            // Act
            var result = await _controller.DeleteOffice(99) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        /// <summary>
        /// ✅ Test: Retrieving doctors from an office should return the correct doctors.
        /// </summary>
        [Fact]
        public async Task GetDoctorsInOffice_ReturnsDoctors_WhenOfficeHasDoctors()
        {
            // ✅ FIX: Ensure Office has required fields
            var office = new Office
            {
                Id = 1,
                Name = "Health Center",
                Address = "789 King St",
                City = "Montreal",
                State = "QC",
                ZipCode = "H3Z2J1"
            };

            // ✅ FIX: Ensure Doctor has required fields
            var doctor = new User
            {
                Id = 1,
                Role = UserRole.Doctor,
                FirstName = "Alice",
                LastName = "Brown",
                Email = "alice@example.com",
                Password = "hashed_password",
                OfficeId = 1
            };

            _context.Offices.Add(office);
            _context.Users.Add(doctor);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetDoctorsInOffice(1) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var doctorsList = result.Value as IEnumerable<object>;
            Assert.NotNull(doctorsList);
            Assert.Single(doctorsList); 
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
