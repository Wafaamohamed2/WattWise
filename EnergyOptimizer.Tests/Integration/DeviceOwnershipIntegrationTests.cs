using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Infrastructure.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EnergyOptimizer.Tests.Integration
{
    public class DeviceOwnershipIntegrationTests
    {
        private EnergyDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<EnergyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new EnergyDbContext(options);
        }

        [Fact]
        public async Task GetDeviceById_UserAccessingOwnDevice_ReturnsDeviceSuccessfully()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userA = "user-a-id";
            var building = new Building { Id = 1, Name = "Building A", UserId = userA };
            var zone = new Zone { Id = 10, Name = "Zone A", Building = building };
            var device = new Device { Id = 100, Name = "User A AC", Zone = zone, IsActive = true };

            context.Buildings.Add(building);
            context.Zones.Add(zone);
            context.Devices.Add(device);
            await context.SaveChangesAsync();

            var repo = new GenericRepository<Device>(context);
            var mockUser = new Mock<ICurrentUserService>();
            mockUser.Setup(u => u.UserId).Returns(userA);
            mockUser.Setup(u => u.RequireUserId()).Returns(userA);

            var handler = new GetDeviceByIdHandler(repo, mockUser.Object);

            // Act
            var response = await handler.Handle(new GetDeviceByIdQuery(100), CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(200);
            var retrievedDevice = response.Details as Device;
            retrievedDevice.Should().NotBeNull();
            retrievedDevice!.Name.Should().Be("User A AC");
        }

        [Fact]
        public async Task GetDeviceById_UserBAccessingUserADevice_ThrowsNotFoundException()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var userA = "user-a-id";
            var userB = "user-b-id";

            var buildingA = new Building { Id = 1, Name = "Building A", UserId = userA };
            var zoneA = new Zone { Id = 10, Name = "Zone A", Building = buildingA };
            var deviceA = new Device { Id = 100, Name = "User A AC", Zone = zoneA, IsActive = true };

            context.Buildings.Add(buildingA);
            context.Zones.Add(zoneA);
            context.Devices.Add(deviceA);
            await context.SaveChangesAsync();

            var repo = new GenericRepository<Device>(context);
            var mockUserB = new Mock<ICurrentUserService>();
            mockUserB.Setup(u => u.UserId).Returns(userB);
            mockUserB.Setup(u => u.RequireUserId()).Returns(userB);

            var handler = new GetDeviceByIdHandler(repo, mockUserB.Object);

            // Act & Assert
            Func<Task> act = async () => await handler.Handle(new GetDeviceByIdQuery(100), CancellationToken.None);
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
