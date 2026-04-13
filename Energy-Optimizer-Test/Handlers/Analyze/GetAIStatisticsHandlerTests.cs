using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AnalysisSpec;
using EnergyOptimizer.Core.Specifications.AnomaliesSpec;
using EnergyOptimizer.Core.Specifications.RecommendationSpec;
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

            _handler = new GetAIStatisticsHandler(
                _mockAnalysis.Object,
                _mockRec.Object,
                _mockAnomaly.Object);
        }

        [Fact]
        public async Task Handle_CalculatesPotentialSavingsFromActiveRecs()
        {
            // Arrange
            _mockAnalysis
                .Setup(r => r.CountAsync(It.IsAny<AnalysisHistoryCountSpec>()))
                .ReturnsAsync(5);

            var recs = new List<EnergyRecommendation>
            {
                new EnergyRecommendation { IsImplemented = false, EstimatedSavingsKWh = 50 },
                new EnergyRecommendation { IsImplemented = true,  EstimatedSavingsKWh = 20 }
            };
            _mockRec
                .Setup(r => r.ListAsync(It.IsAny<RecommendationsFilterSpec>()))
                .ReturnsAsync(recs);

            var anomalies = new List<DetectedAnomaly>
            {
                new DetectedAnomaly { IsResolved = false, Severity = "High",   DeviceId = 1 },
                new DetectedAnomaly { IsResolved = false, Severity = "Critical", DeviceId = 2 },
                new DetectedAnomaly { IsResolved = true,  Severity = "Low",    DeviceId = 1 }
            };
            _mockAnomaly
                .Setup(r => r.ListAsync(It.IsAny<AnomaliesFilterSpec>()))
                .ReturnsAsync(anomalies);

            // Act
            var result = await _handler.Handle(new GetAIStatisticsQuery(), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var json = JsonSerializer.Serialize(result.Details);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Potential savings = only non-implemented recs
            root.GetProperty("recommendations")
                .GetProperty("totalPotentialSavings")
                .GetDouble()
                .Should().Be(50);

            // Realized savings = only implemented recs
            root.GetProperty("recommendations")
                .GetProperty("totalRealizedSavings")
                .GetDouble()
                .Should().Be(20);

            // Active recs = non-implemented count
            root.GetProperty("recommendations")
                .GetProperty("active")
                .GetInt32()
                .Should().Be(1);

            // Unresolved anomalies
            root.GetProperty("anomalies")
                .GetProperty("unresolved")
                .GetInt32()
                .Should().Be(2);

            // Devices affected = distinct DeviceIds
            root.GetProperty("anomalies")
                .GetProperty("devicesAffected")
                .GetInt32()
                .Should().Be(2);
        }

        [Fact]
        public async Task Handle_WhenNoData_ReturnsZeroStats()
        {
            // Arrange
            _mockAnalysis
                .Setup(r => r.CountAsync(It.IsAny<AnalysisHistoryCountSpec>()))
                .ReturnsAsync(0);

            _mockRec
                .Setup(r => r.ListAsync(It.IsAny<RecommendationsFilterSpec>()))
                .ReturnsAsync(new List<EnergyRecommendation>());

            _mockAnomaly
                .Setup(r => r.ListAsync(It.IsAny<AnomaliesFilterSpec>()))
                .ReturnsAsync(new List<DetectedAnomaly>());

            // Act
            var result = await _handler.Handle(new GetAIStatisticsQuery(), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var json = JsonSerializer.Serialize(result.Details);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("analyses").GetProperty("total").GetInt32().Should().Be(0);
            root.GetProperty("recommendations").GetProperty("totalPotentialSavings").GetDouble().Should().Be(0);
            root.GetProperty("anomalies").GetProperty("unresolved").GetInt32().Should().Be(0);
        }

        [Fact]
        public async Task Handle_CountAsync_CalledTwice_ForTotalAndRecent()
        {
            // Arrange — verify the handler calls CountAsync exactly twice:
            // once for total analyses and once for last-30-days analyses
            _mockAnalysis
                .Setup(r => r.CountAsync(It.IsAny<AnalysisHistoryCountSpec>()))
                .ReturnsAsync(3);

            _mockRec
                .Setup(r => r.ListAsync(It.IsAny<RecommendationsFilterSpec>()))
                .ReturnsAsync(new List<EnergyRecommendation>());

            _mockAnomaly
                .Setup(r => r.ListAsync(It.IsAny<AnomaliesFilterSpec>()))
                .ReturnsAsync(new List<DetectedAnomaly>());

            // Act
            await _handler.Handle(new GetAIStatisticsQuery(), CancellationToken.None);

            // Assert — CountAsync called twice (total + last30Days)
            _mockAnalysis.Verify(r => r.CountAsync(It.IsAny<AnalysisHistoryCountSpec>()), Times.Exactly(2));
        }
    }
}