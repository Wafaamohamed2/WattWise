using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Devices
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
        public async Task Handle_ActiveDevice_DeactivatesAndNotifiesHub()
        {
            // Arrange
            var device = new Device { Id = 1, IsActive = true };

            _mockDeviceRepo
                .Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                .ReturnsAsync(device);

            // Act
            var result = await _handler.Handle(
                new ToggleDeviceCommand(1),
                CancellationToken.None);

            // Assert — device flipped from active to inactive
            device.IsActive.Should().BeFalse();

            // After 
            _mockDeviceRepo.Verify(r => r.Update(device), Times.Once);
            _mockDeviceRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

            _mockHubService.Verify(
                h => h.NotifyDeviceStatusChanged(1, false),
                Times.Once);

            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Handle_InactiveDevice_ActivatesAndNotifiesHub()
        {
            // Arrange
            var device = new Device { Id = 2, IsActive = false };

            _mockDeviceRepo
                .Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                .ReturnsAsync(device);

            // Act
            var result = await _handler.Handle(
                new ToggleDeviceCommand(2),
                CancellationToken.None);

            // Assert
            device.IsActive.Should().BeTrue();
            _mockHubService.Verify(
                h => h.NotifyDeviceStatusChanged(2, true),
                Times.Once);

            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task Handle_DeviceNotFound_ThrowsNotFoundException()
        {
            // Arrange
            _mockDeviceRepo
                .Setup(r => r.GetEntityWithSpec(It.IsAny<DeviceWithDetailsSpec>()))
                .ReturnsAsync((Device?)null);

            // Act
            Func<Task> act = async () =>
                await _handler.Handle(new ToggleDeviceCommand(999), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*999*");

            _mockHubService.Verify(
                h => h.NotifyDeviceStatusChanged(It.IsAny<int>(), It.IsAny<bool>()),
                Times.Never);
        }
    }
}
