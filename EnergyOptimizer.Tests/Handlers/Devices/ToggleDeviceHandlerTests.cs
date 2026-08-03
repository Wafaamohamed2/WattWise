using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.Devices.Commands;
using EnergyOptimizer.Core.Features.Devices.Handlers;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Devices
{
    public class ToggleDeviceHandlerTests
    {
        private readonly Mock<IGenericRepository<Device>> _mockRepo;
        private readonly Mock<IEnergyHubService> _mockHubService;
        private readonly Mock<ICurrentUserService> _mockCurrentUserService;
        private readonly ToggleDeviceHandler _handler;

        public ToggleDeviceHandlerTests()
        {
            _mockRepo = new Mock<IGenericRepository<Device>>();
            _mockHubService = new Mock<IEnergyHubService>();
            _mockCurrentUserService = new Mock<ICurrentUserService>();
            _mockCurrentUserService.Setup(u => u.UserId).Returns("user-123");
            _mockCurrentUserService.Setup(u => u.RequireUserId()).Returns("user-123");

            _handler = new ToggleDeviceHandler(_mockRepo.Object, _mockHubService.Object, _mockCurrentUserService.Object);
        }

        [Fact]
        public async Task Handle_ValidId_TogglesIsActiveAndReturnsSuccess()
        {
            // Arrange
            var deviceId = 1;
            var device = new Device { Id = deviceId, IsActive = false };

            _mockRepo.Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                     .ReturnsAsync(device);

            var command = new ToggleDeviceCommand(deviceId);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            device.IsActive.Should().BeTrue();
            _mockRepo.Verify(r => r.Update(device), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockHubService.Verify(h => h.NotifyDeviceStatusChanged(deviceId, true), Times.Once);

            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("activated");
        }
    }
}
