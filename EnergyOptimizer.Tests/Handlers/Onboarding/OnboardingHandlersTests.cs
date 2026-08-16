using EnergyOptimizer.Core.DTOs.OnboardingDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.Onboarding.Commands;
using EnergyOptimizer.Core.Features.Onboarding.Handlers;
using EnergyOptimizer.Core.Features.Onboarding.Queries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Specifications.DeviceTemplateSpec;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Onboarding
{
    public class OnboardingHandlersTests
    {
        private readonly Mock<IGenericRepository<Building>> _buildingRepoMock = new();
        private readonly Mock<IGenericRepository<Zone>> _zoneRepoMock = new();
        private readonly Mock<IGenericRepository<Device>> _deviceRepoMock = new();
        private readonly Mock<IGenericRepository<DeviceTemplate>> _templateRepoMock = new();
        private readonly Mock<ICurrentUserService> _currentUserMock = new();

        public OnboardingHandlersTests()
        {
            _currentUserMock.Setup(u => u.RequireUserId()).Returns("user-123");
        }

        [Fact]
        public async Task CreateBuildingWithTypeHandler_ShouldCreateBuilding_WithOnboardingFalse()
        {
            // Arrange
            var handler = new CreateBuildingWithTypeHandler(_buildingRepoMock.Object, _currentUserMock.Object);
            var dto = new CreateBuildingWithTypeDto
            {
                Name = "New Office",
                Address = "Tech Park",
                Type = BuildingType.Company
            };
            var command = new CreateBuildingWithTypeCommand(dto);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(200);
            _buildingRepoMock.Verify(r => r.Add(It.Is<Building>(b => 
                b.Name == "New Office" && 
                b.Type == BuildingType.Company && 
                b.UserId == "user-123" && 
                !b.IsOnboardingComplete)), Times.Once);
            _buildingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GetDeviceTemplatesHandler_ShouldReturnTemplatesForBuildingType()
        {
            // Arrange
            var templates = new List<DeviceTemplate>
            {
                new DeviceTemplate { Id = 1, BuildingType = BuildingType.Home, SuggestedName = "AC", DeviceType = DeviceType.AirConditioner, SuggestedZoneType = ZoneType.LivingRoom }
            };

            _templateRepoMock
                .Setup(r => r.ListAsync(It.IsAny<DeviceTemplateByBuildingTypeSpec>()))
                .ReturnsAsync(templates);

            var handler = new GetDeviceTemplatesHandler(_templateRepoMock.Object);
            var query = new GetDeviceTemplatesQuery(BuildingType.Home);

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(200);
            response.Details.Should().BeEquivalentTo(templates);
        }

        [Fact]
        public async Task CompleteOnboardingHandler_ShouldCreateZonesDevices_WithValidZoneType_AndSetOnboardingComplete()
        {
            // Arrange
            var building = new Building
            {
                Id = 10,
                Name = "Smart Home",
                UserId = "user-123",
                IsOnboardingComplete = false
            };

            _buildingRepoMock
                .Setup(r => r.GetEntityWithSpec(It.IsAny<BuildingOwnedByUserSpec>()))
                .ReturnsAsync(building);

            var handler = new CompleteOnboardingHandler(
                _buildingRepoMock.Object,
                _zoneRepoMock.Object,
                _deviceRepoMock.Object,
                _currentUserMock.Object);

            var dto = new CompleteOnboardingDto
            {
                BuildingId = 10,
                Devices = new List<DeviceSelectionDto>
                {
                    new DeviceSelectionDto
                    {
                        DeviceType = DeviceType.AirConditioner,
                        Name = "Living AC",
                        Quantity = 1,
                        RatedPowerKW = 1.8,
                        ZoneName = "Living Room",
                        ZoneType = ZoneType.LivingRoom
                    }
                }
            };

            var command = new CompleteOnboardingCommand(dto);

            // Act
            var response = await handler.Handle(command, CancellationToken.None);

            // Assert
            response.StatusCode.Should().Be(200);
            building.IsOnboardingComplete.Should().BeTrue();
            _zoneRepoMock.Verify(r => r.Add(It.Is<Zone>(z => z.Name == "Living Room" && z.BuildingId == 10 && z.Type == ZoneType.LivingRoom)), Times.Once);
            _deviceRepoMock.Verify(r => r.Add(It.Is<Device>(d => d.Name == "Living AC" && d.Type == DeviceType.AirConditioner)), Times.Once);
            _buildingRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
