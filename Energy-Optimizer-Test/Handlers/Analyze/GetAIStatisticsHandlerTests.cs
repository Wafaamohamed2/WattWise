using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using System.Text.Json;

namespace EnergyOptimizer.Tests.Handlers.Analyze
{
    public class GetAIStatisticsHandlerTests
    {
        private readonly Mock<IGenericRepository<EnergyAnalysis>> _mockAnalysis;
        private readonly Mock<IGenericRepository<EnergyRecommendation>> _mockRec;
        private readonly Mock<IGenericRepository<DetectedAnomaly>> _mockAnomaly;
        private readonly GetAIStatisticsHandler _handler;

        public GetAIStatisticsHandlerTests()
        {
            _mockAnalysis = new Mock<IGenericRepository<EnergyAnalysis>>();
            _mockRec = new Mock<IGenericRepository<EnergyRecommendation>>();
            _mockAnomaly = new Mock<IGenericRepository<DetectedAnomaly>>();

            _handler = new GetAIStatisticsHandler(_mockAnalysis.Object, _mockRec.Object, _mockAnomaly.Object);
        }

        [Fact]
        public async Task Handle_CalculatesAllStatsCorrectly()
        {
            // Arrange
            var analyses = new List<EnergyAnalysis> { new EnergyAnalysis { AnalysisDate = DateTime.UtcNow } };
            _mockAnalysis.Setup(r => r.ListAllAsync()).ReturnsAsync(analyses);

            var recs = new List<EnergyRecommendation> { new EnergyRecommendation { IsImplemented = false, EstimatedSavingsKWh = 50 } };
            _mockRec.Setup(r => r.ListAllAsync()).ReturnsAsync(recs);

            _mockAnomaly.Setup(r => r.ListAllAsync()).ReturnsAsync(new List<DetectedAnomaly>());

            // Act
            var result = await _handler.Handle(new GetAIStatisticsQuery(), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var json = JsonSerializer.Serialize(result.Details);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var potentialSavings = root.GetProperty("recommendations")
                                       .GetProperty("totalPotentialSavings")
                                       .GetDouble();

            potentialSavings.Should().Be(50);
        }
    }
}