using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers;
using EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AnalysisSpec;
using EnergyOptimizer.API.DTOs.Gemini;
using FluentAssertions;
using Moq;
using System.Text.Json;
using DetectedAnomaly = EnergyOptimizer.API.DTOs.Gemini.DetectedAnomaly;

namespace EnergyOptimizer.Tests.Handlers.Analyze
{
    public class AnalyzeAndAnomaliesTests
    {
        [Fact]
        public async Task GetAnalysisHistory_ReturnsPaginatedData()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<EnergyAnalysis>>();

            mockRepo
                .Setup(r => r.CountAsync(It.IsAny<ISpecification<EnergyAnalysis>>()))
                .ReturnsAsync(1);

            var data = new List<EnergyAnalysis>
            {
                new EnergyAnalysis { Id = 1, AnalysisType = "Pattern", AnalysisDate = DateTime.UtcNow }
            };
            mockRepo
                .Setup(r => r.ListAsync(It.IsAny<AnalysisHistorySpec>()))
                .ReturnsAsync(data);

            var handler = new GetAnalysisHistoryHandler(mockRepo.Object);
            var query = new GetAnalysisHistoryQuery(Page: 1, PageSize: 10, null, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = JsonSerializer.Serialize(result.Details);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("count").GetInt32().Should().Be(1);
            root.GetProperty("page").GetInt32().Should().Be(1);
            root.GetProperty("totalPages").GetInt32().Should().Be(1);
        }

        [Fact]
        public async Task GetAnalysisHistory_EmptyResult_ReturnsTotalPagesZero()
        {
            // Arrange
            var mockRepo = new Mock<IGenericRepository<EnergyAnalysis>>();

            mockRepo
                 .Setup(r => r.CountAsync(It.IsAny<ISpecification<EnergyAnalysis>>()))
                 .ReturnsAsync(0);

            mockRepo
                 .Setup(r => r.ListAsync(It.IsAny<ISpecification<EnergyAnalysis>>()))
                 .ReturnsAsync(new List<EnergyAnalysis>());

            var handler = new GetAnalysisHistoryHandler(mockRepo.Object);
            var query = new GetAnalysisHistoryQuery(1, 10, null, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var json = JsonSerializer.Serialize(result.Details);
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.GetProperty("count").GetInt32().Should().Be(0);
        }

        [Fact]
        public async Task DetectDeviceAnomalies_CallsPatternServiceWithCorrectArgs()
        {
            // Arrange
            var mockService = new Mock<IPatternDetectionService>();
            mockService
                .Setup(s => s.DetectDeviceAnomalies(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new AnomalyDetectionResult
                {
                    HasAnomalies = false,
                    Anomalies = new List<DetectedAnomaly>(),
                    Analysis = "No anomalies detected"
                });

            var handler = new DetectDeviceAnomaliesHandler(mockService.Object);

            // Act
            var result = await handler.Handle(
                new DetectDeviceAnomaliesCommand(DeviceId: 1, Days: 7),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            mockService.Verify(s => s.DetectDeviceAnomalies(1, 7), Times.Once);
        }

        [Fact]
        public async Task DetectDeviceAnomalies_WhenAnomaliesFound_ReturnsCorrectCount()
        {
            // Arrange
            var mockService = new Mock<IPatternDetectionService>();
            var anomalies = new List<DetectedAnomaly>
            {
                new DetectedAnomaly { Severity = "High", Description = "Spike detected" },
                new DetectedAnomaly { Severity = "Medium", Description = "Unusual usage" }
            };

            mockService
                .Setup(s => s.DetectDeviceAnomalies(5, 14))
                .ReturnsAsync(new AnomalyDetectionResult
                {
                    HasAnomalies = true,
                    Anomalies = anomalies,
                    Analysis = "2 anomalies found"
                });

            var handler = new DetectDeviceAnomaliesHandler(mockService.Object);

            // Act
            var result = await handler.Handle(
                new DetectDeviceAnomaliesCommand(DeviceId: 5, Days: 14),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var json = JsonSerializer.Serialize(result.Details);
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.GetProperty("hasAnomalies").GetBoolean().Should().BeTrue();
            doc.RootElement.GetProperty("anomaliesCount").GetInt32().Should().Be(2);
        }
    }
}