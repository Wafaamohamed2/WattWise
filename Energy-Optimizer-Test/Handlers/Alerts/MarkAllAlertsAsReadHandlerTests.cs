using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using FluentAssertions;
using Moq;

namespace Energy_Optimizer_Test.Handlers.Alerts
{
    public class MarkAllAlertsAsReadHandlerTests
    {
        private readonly Mock<IGenericRepository<Alert>> _mockRepo; 
        private readonly MarkAllAlertsAsReadHandler _handler;

        public MarkAllAlertsAsReadHandlerTests()
        {
            _mockRepo = new Mock<IGenericRepository<Alert>>();
            _handler = new MarkAllAlertsAsReadHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_UnreadAlertsExist_MarksThemAsRead()
        {
            // Arrange
            var unreadAlerts = new List<Alert> { new Alert { IsRead = false } };

            _mockRepo.Setup(r => r.ListAsync(It.IsAny<AlertCountSpec>()))
                     .ReturnsAsync(unreadAlerts);

            // Act
            var result = await _handler.Handle(new MarkAllAlertsAsReadCommand(), CancellationToken.None);

            // Assert
            unreadAlerts.All(a => a.IsRead).Should().BeTrue();

            _mockRepo.Verify(r => r.Update(It.IsAny<Alert>()), Times.AtLeastOnce);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
