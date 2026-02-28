using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries;
using EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Anomalies
{
    public class ResolveAnomalyHandlerTests
    {
        private readonly Mock<IGenericRepository<DetectedAnomaly>> _mockRepo;
        private readonly ResolveAnomalyHandler _handler;

        public ResolveAnomalyHandlerTests()
        {
            _mockRepo = new Mock<IGenericRepository<DetectedAnomaly>>();
            _handler = new ResolveAnomalyHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_UpdatesAnomalyDetails()
        {
            // Arrange
            var anomaly = new DetectedAnomaly { Id = 1, IsResolved = false };
            var notes = "Problem fixed in the circuit breaker";
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(anomaly);

            // Act
            var result = await _handler.Handle(new ResolveAnomalyCommand(1, notes), CancellationToken.None);

            // Assert
            anomaly.IsResolved.Should().BeTrue();
            anomaly.ResolutionNotes.Should().Be(notes);
            result.StatusCode.Should().Be(200);
        }
    }
}