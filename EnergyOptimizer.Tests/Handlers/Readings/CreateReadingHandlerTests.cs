using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers;
using EnergyOptimizer.Core.Contracts;
using MassTransit;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Readings
{
    public class CreateReadingHandlerTests
    {
        private readonly Mock<IPublishEndpoint> _mockPublish;
        private readonly CreateReadingHandler _handler;

        public CreateReadingHandlerTests()
        {
            _mockPublish = new Mock<IPublishEndpoint>();
            _handler = new CreateReadingHandler(_mockPublish.Object);
        }

        [Fact]
        public async Task Handle_ValidReading_PublishesToQueue()
        {
            // Arrange
            var dto = new CreateReadingDto { DeviceId = 1, PowerConsumptionKW = 5.0m, Voltage = 220, Current = 2.5m, Temperature = 25.0 };
            var commandObj = new CreateReadingCommand(dto);

            // Act
            var result = await _handler.Handle(commandObj, CancellationToken.None);

            // Assert
            _mockPublish.Verify(p => p.Publish(It.IsAny<EnergyReadingReceivedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            result.StatusCode.Should().Be(202);
        }
    }
}
