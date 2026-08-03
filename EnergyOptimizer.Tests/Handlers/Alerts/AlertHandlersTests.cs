using AutoMapper;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Alerts.Handlers;
using EnergyOptimizer.Core.Features.Alerts.Queries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Alerts
{
    public class AlertHandlersTests
    {
        private readonly Mock<IGenericRepository<Alert>> _mockAlertRepo;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ICurrentUserService> _mockUserService;

        public AlertHandlersTests()
        {
            _mockAlertRepo = new Mock<IGenericRepository<Alert>>();
            _mockMapper = new Mock<IMapper>();
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(u => u.RequireUserId()).Returns("user-123");
        }

        [Fact]
        public async Task GetAlerts_ReturnsMappedData()
        {
            // Arrange
            var alerts = new List<Alert> { new Alert { Id = 1 } };
            _mockAlertRepo.Setup(r => r.ListAsync(It.IsAny<AlertsWithFiltersSpec>())).ReturnsAsync(alerts);
            _mockMapper.Setup(m => m.Map<List<AlertDto>>(alerts)).Returns(new List<AlertDto> { new AlertDto { Id = 1 } });

            var query = new GetAlertsQuery(null, null, null, null, null, 1, 10);
            var handler = new GetAlertsHandler(_mockAlertRepo.Object, _mockMapper.Object, _mockUserService.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("successfully");
        }

        [Fact]
        public async Task GetAlertById_NotFound_ThrowsException()
        {
            // Arrange
            _mockAlertRepo.Setup(r => r.GetEntityWithSpec(It.IsAny<AlertOwnedByUserSpec>())).ReturnsAsync((Alert?)null);
            var handler = new GetAlertByIdHandler(_mockAlertRepo.Object, _mockMapper.Object, _mockUserService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(new GetAlertByIdQuery(99), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
