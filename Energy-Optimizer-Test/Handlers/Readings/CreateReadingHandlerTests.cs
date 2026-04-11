using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Readings
{
    public class CreateReadingHandlerTests
    {
        private readonly Mock<IGenericRepository<EnergyReading>> _mockRepo;
        private readonly Mock<IEnergyHubService> _mockHub;
        private readonly CreateReadingHandler _handler;

        public CreateReadingHandlerTests()
        {
            _mockRepo = new Mock<IGenericRepository<EnergyReading>>();
            _mockHub = new Mock<IEnergyHubService>();
            _handler = new CreateReadingHandler(_mockRepo.Object, _mockHub.Object);
        }

        [Fact]
        public async Task Handle_ValidReading_SavesAndBroadcasts()
        {
            // Arrange
            var dto = new CreateReadingDto { DeviceId = 1, PowerConsumptionKW = 5.0m };
            var command = new CreateReadingCommand(dto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _mockRepo.Verify(r => r.Add(It.IsAny<EnergyReading>()), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            _mockHub.Verify(h => h.NotifyNewReading(It.IsAny<object>()), Times.Once);
            result.StatusCode.Should().Be(201);
        }
    }
}