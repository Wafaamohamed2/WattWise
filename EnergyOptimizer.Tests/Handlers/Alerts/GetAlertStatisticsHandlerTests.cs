using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Alerts
{
    public class GetAlertStatisticsHandlerTests
    {
        private readonly Mock<IGenericRepository<Alert>> _mockAlertRepo;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly GetAlertStatisticsHandler _handler;

        public GetAlertStatisticsHandlerTests()
        {
            _mockAlertRepo = new Mock<IGenericRepository<Alert>>();
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(u => u.RequireUserId()).Returns("user-123");
            _handler = new GetAlertStatisticsHandler(_mockAlertRepo.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsCorrectStatistics()
        {
            // Arrange
            _mockAlertRepo.Setup(repo => repo.CountAsync(It.Is<AlertCountSpec>(s => !s.IsRead.HasValue && !s.Severity.HasValue)))
                          .ReturnsAsync(10); // Total

            _mockAlertRepo.Setup(repo => repo.CountAsync(It.Is<AlertCountSpec>(s => s.IsRead == false)))
                          .ReturnsAsync(4); // Unread

            _mockAlertRepo.Setup(repo => repo.CountAsync(It.Is<AlertCountSpec>(s => s.Severity == AlertSeverity.Critical)))
                          .ReturnsAsync(3); // Critical

            _mockAlertRepo.Setup(repo => repo.CountAsync(It.Is<AlertCountSpec>(s => s.Severity == AlertSeverity.Warning)))
                          .ReturnsAsync(2); // Warning

            _mockAlertRepo.Setup(repo => repo.CountAsync(It.Is<AlertCountSpec>(s => s.Severity == AlertSeverity.Info)))
                          .ReturnsAsync(1); // Info

            var query = new GetAlertStatisticsQuery(StartDate: null, Days: 7);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Statistics retrieved successfully");

            var stats = result.Details as AlertStatistics;
            stats.Should().NotBeNull();
            stats!.TotalAlerts.Should().Be(10);
            stats.UnreadAlerts.Should().Be(4);
            stats.CriticalAlerts.Should().Be(3);
            stats.WarningAlerts.Should().Be(2);
            stats.InfoAlerts.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoAlerts_ReturnsZeroStatistics()
        {
            // Arrange
            _mockAlertRepo.Setup(repo => repo.CountAsync(It.IsAny<AlertCountSpec>()))
                         .ReturnsAsync(0);

            var query = new GetAlertStatisticsQuery(null, 7);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            var stats = result.Details as AlertStatistics;
            stats!.TotalAlerts.Should().Be(0);
            stats.UnreadAlerts.Should().Be(0);
            stats.CriticalAlerts.Should().Be(0);
            stats.WarningAlerts.Should().Be(0);
            stats.InfoAlerts.Should().Be(0);
        }
    }
}
