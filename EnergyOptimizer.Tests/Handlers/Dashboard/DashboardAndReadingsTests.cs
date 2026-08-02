using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers;
using EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Tests.Helpers;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Dashboard
{
    public class DashboardAndReadingsTests
    {
        private readonly Mock<ICurrentUserService> _mockUserService;

        public DashboardAndReadingsTests()
        {
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(u => u.RequireUserId()).Returns("user-123");
        }

        [Fact]
        public async Task GetHourlyConsumption_Returns24Hours()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<EnergyReading>>();
            mockRepo.Setup(r => r.GetQueryable()).Returns(new List<EnergyReading>().AsAsyncQueryable());
            var handler = new GetHourlyConsumptionHandler(mockRepo.Object, _mockUserService.Object);
            var query = new GetHourlyConsumptionQuery(DateTime.UtcNow.ToString("yyyy-MM-dd"));

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            var list = result.Details as List<EnergyOptimizer.Core.DTOs.ReadingsDTOs.HourlyConsumptionDto>;
            list!.Count.Should().Be(24);
        }

        [Fact]
        public async Task GetTopConsumers_ReturnsCorrectJson()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<EnergyReading>>();
            mockRepo.Setup(r => r.ListAsync(It.IsAny<LatestReadingsSpec>())).ReturnsAsync(new List<EnergyReading>());
            var handler = new GetTopConsumersHandler(mockRepo.Object, _mockUserService.Object);

            // Act
            var result = await handler.Handle(new GetTopConsumersQuery(5, ""), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task GetLatestReadings_ReturnsFormattedData()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<EnergyReading>>();
            mockRepo.Setup(r => r.ListAsync(It.IsAny<LatestReadingsSpec>())).ReturnsAsync(new List<EnergyReading> { new EnergyReading { Id = 1 } });
            var handler = new GetLatestReadingsHandler(mockRepo.Object, _mockUserService.Object);
            var query = new GetLatestReadingsQuery(10, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);
            // Assert
            result.StatusCode.Should().Be(200);
        }
    }
}
