using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace EnergyOptimizer.Tests.Handlers.Dashboard
{
    public class GetDashboardOverviewHandlerTests
    {
        private readonly Mock<IGenericRepository<Device>> _mockDeviceRepo;
        private readonly Mock<IGenericRepository<EnergyReading>> _mockReadingRepo;
        private readonly Mock<IGenericRepository<Alert>> _mockAlertRepo;
        private readonly Mock<IGenericRepository<Zone>> _mockZoneRepo;
        private readonly GetDashboardOverviewHandler _handler;

        public GetDashboardOverviewHandlerTests()
        {
            _mockDeviceRepo = new Mock<IGenericRepository<Device>>();
            _mockReadingRepo = new Mock<IGenericRepository<EnergyReading>>();
            _mockAlertRepo = new Mock<IGenericRepository<Alert>>();
            _mockZoneRepo = new Mock<IGenericRepository<Zone>>();

            _handler = new GetDashboardOverviewHandler(
                _mockDeviceRepo.Object, _mockReadingRepo.Object,
                _mockAlertRepo.Object, _mockZoneRepo.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAggregatedOverviewData()
        {
            // Arrange
            _mockDeviceRepo.Setup(r => r.CountAsync(It.IsAny<CountActiveDevicesSpec>())).ReturnsAsync(10);
            _mockZoneRepo.Setup(r => r.CountAsync(It.IsAny<ZoneCountSpec>())).ReturnsAsync(3);
            _mockAlertRepo.Setup(r => r.CountAsync(It.IsAny<AlertCountSpec>())).ReturnsAsync(5);

            var readings = new List<EnergyReading>
            {
                new EnergyReading { PowerConsumptionKW = 2.5m },
                new EnergyReading { PowerConsumptionKW = 1.5m }
            };
            _mockReadingRepo.Setup(r => r.ListAsync(It.IsAny<LatestReadingsSpec>())).ReturnsAsync(readings);

            // Act
            var result = await _handler.Handle(new GetDashboardOverviewQuery(), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(result.Details, options);
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

            data.Should().ContainKey("totalDevices");
            data["totalDevices"].ToString().Should().Be("10");

            data.Should().ContainKey("currentPowerUsageKW");
            data["currentPowerUsageKW"].ToString().Should().Be("4");
        }
    }
}
