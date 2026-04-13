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
        public async Task Handle_UnreadAlertsExist_SetsIsReadTrueAndPersists()
        {
            // Arrange
            var unreadAlerts = new List<Alert>
            {
                new Alert { Id = 1, IsRead = false },
                new Alert { Id = 2, IsRead = false }
            };

            _mockRepo
                .Setup(r => r.ListAsync(It.IsAny<AlertCountSpec>()))
                .ReturnsAsync(unreadAlerts);

            // Act
            var result = await _handler.Handle(
                new MarkAllAlertsAsReadCommand(),
                CancellationToken.None);

            // Assert — all alerts should be marked as read in memory
            unreadAlerts.Should().AllSatisfy(a => a.IsRead.Should().BeTrue());

            // After 
            _mockRepo.Verify(r => r.UpdateRange(unreadAlerts), Times.Once);

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);

            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("2");
        }

        [Fact]
        public async Task Handle_NoUnreadAlerts_SavesWithZeroUpdates()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.ListAsync(It.IsAny<AlertCountSpec>()))
                .ReturnsAsync(new List<Alert>());

            // Act
            var result = await _handler.Handle(
                new MarkAllAlertsAsReadCommand(),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("0");

            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}