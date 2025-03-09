using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CareBridgeBackend.Controllers;
using CareBridgeBackend.Data;
using CareBridgeBackend.DTOs;
using System.Threading.Tasks;

namespace CareBridgeBackend.Tests.Controllers
{
    public class AppointmentControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly AppointmentsController _controller;

        public AppointmentControllerTests()
        {
            // Use InMemoryDatabase instead of Moq
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new AppointmentsController(_context);
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _controller.CreateAppointment(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result); // ✅ Should pass now
        }
    }
}
