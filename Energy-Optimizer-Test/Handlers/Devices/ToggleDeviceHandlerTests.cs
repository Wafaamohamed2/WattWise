using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using FluentAssertions;
using Moq;

namespace Energy_Optimizer_Test.Handlers.Devices
{
    public class ToggleDeviceHandlerTests
    {
        private readonly Mock<IGenericRepository<Device>> _mockDeviceRepo;
        private readonly Mock<IEnergyHubService> _mockHubService;
        private readonly ToggleDeviceHandler _handler;

        public ToggleDeviceHandlerTests()
        {
            _mockDeviceRepo = new Mock<IGenericRepository<Device>>();
            _mockHubService = new Mock<IEnergyHubService>();
            _handler = new ToggleDeviceHandler(_mockDeviceRepo.Object, _mockHubService.Object);
        }

        [Fact]
        public async Task Handle_DeviceExists_TogglesStatusAndNotifiesHub()
        {
            // Arrange
            var deviceId = 1;
            var device = new Device { Id = deviceId, IsActive = true };
            _mockDeviceRepo.Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                           .ReturnsAsync(device);

            // Act
            var result = await _handler.Handle(new ToggleDeviceCommand(deviceId), CancellationToken.None);

            // Assert
            device.IsActive.Should().BeFalse(); 
            _mockDeviceRepo.Verify(r => r.UpdateAsync(device), Times.Once);
            _mockHubService.Verify(h => h.NotifyDeviceStatusChanged(deviceId, false), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Handle_DeviceNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _mockDeviceRepo.Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                           .ReturnsAsync((Device)null);

            // Act
            Func<Task> act = async () => await _handler.Handle(new ToggleDeviceCommand(999), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

    }
}
