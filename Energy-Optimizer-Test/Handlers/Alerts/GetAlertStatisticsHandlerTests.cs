using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using FluentAssertions;
using Moq;

namespace Energy_Optimizer_Test.Handlers.Alerts
{
    public class GetAlertStatisticsHandlerTests
    {
        private readonly Mock<IGenericRepository<Alert>> _mockAlertRepo;
        private readonly GetAlertStatisticsHandler _handler;

        public GetAlertStatisticsHandlerTests()
        {
            _mockAlertRepo = new Mock<IGenericRepository<Alert>>();
            _handler = new GetAlertStatisticsHandler(_mockAlertRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsCorrectStatistics()
        {
            // Arrange
            var alerts = new List<Alert>
            {
                new Alert { Id = 1, IsRead = false, Severity = 3 }, // Critical & Unread
                new Alert { Id = 2, IsRead = true, Severity = 2 },  // Warning & Read
                new Alert { Id = 3, IsRead = false, Severity = 1 }  // Info & Unread
            };
            _mockAlertRepo.Setup(repo => repo.ListAsync(It.IsAny<AlertsByDateSpec>()))
                         .ReturnsAsync(alerts);

            var query = new GetAlertStatisticsQuery(StartDate: null, Days: 7);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Statistics retrieved successfully");

            var stats = result.Details as AlertStatistics;
            stats.Should().NotBeNull();
            stats!.TotalAlerts.Should().Be(3);
            stats.UnreadAlerts.Should().Be(2);
            stats.CriticalAlerts.Should().Be(1);
            stats.WarningAlerts.Should().Be(1);
            stats.InfoAlerts.Should().Be(1);
        }
        [Fact]
        public async Task Handle_NoAlerts_ReturnsZeroStatistics()
        {
            // Arrange
            _mockAlertRepo.Setup(repo => repo.ListAsync(It.IsAny<AlertsByDateSpec>()))
                         .ReturnsAsync(new List<Alert>());

            var query = new GetAlertStatisticsQuery(null, 7);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var stats = result.Details as AlertStatistics;
            stats!.TotalAlerts.Should().Be(0);
            stats.UnreadAlerts.Should().Be(0);
        }
    }
}
