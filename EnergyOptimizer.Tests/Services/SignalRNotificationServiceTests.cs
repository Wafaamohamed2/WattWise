using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Service.Hubs;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnergyOptimizer.Tests.Services
{
    public class SignalRNotificationServiceTests
    {
        private readonly Mock<IHubContext<NotificationHub>> _mockHubContext;
        private readonly Mock<IHubClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;
        private readonly Mock<ILogger<SignalRNotificationService>> _mockLogger;

        public SignalRNotificationServiceTests()
        {
            _mockHubContext = new Mock<IHubContext<NotificationHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();
            _mockLogger = new Mock<ILogger<SignalRNotificationService>>();

            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.All).Returns(_mockClientProxy.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);
        }

        [Fact]
        public async Task BroadcastAlertAsync_SendsAlertToAllConnectedClients()
        {
            // Arrange
            var service = new SignalRNotificationService(_mockHubContext.Object, _mockLogger.Object);
            var alertDto = new AlertDto
            {
                Id = 101,
                DeviceName = "HVAC Motor",
                ZoneName = "Zone A",
                AlertType = "HighConsumption",
                Message = "High power usage detected",
                Severity = AlertSeverity.Critical,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await service.BroadcastAlertAsync(alertDto, CancellationToken.None);

            // Assert
            _mockClients.Verify(c => c.All, Times.Once);
            _mockClientProxy.Verify(
                p => p.SendCoreAsync("ReceiveAlert", It.Is<object[]>(o => o.Length == 1 && o[0] == alertDto), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendAlertToUserAsync_SendsAlertToUserGroup()
        {
            // Arrange
            var service = new SignalRNotificationService(_mockHubContext.Object, _mockLogger.Object);
            var userId = "user-abc-123";
            var alertDto = new AlertDto { Id = 202, Message = "Test User Alert" };

            // Act
            await service.SendAlertToUserAsync(userId, alertDto, CancellationToken.None);

            // Assert
            _mockClients.Verify(c => c.Group("User_user-abc-123"), Times.Once);
            _mockClientProxy.Verify(
                p => p.SendCoreAsync("ReceiveAlert", It.Is<object[]>(o => o.Length == 1 && o[0] == alertDto), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SendUnreadCountUpdateAsync_SendsCountToUserGroup()
        {
            // Arrange
            var service = new SignalRNotificationService(_mockHubContext.Object, _mockLogger.Object);
            var userId = "user-xyz";
            var unreadCount = 5;

            // Act
            await service.SendUnreadCountUpdateAsync(userId, unreadCount, CancellationToken.None);

            // Assert
            _mockClients.Verify(c => c.Group("User_user-xyz"), Times.Once);
            _mockClientProxy.Verify(
                p => p.SendCoreAsync("UpdateUnreadCount", It.Is<object[]>(o => o.Length == 1 && (int)o[0] == 5), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
