using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers;
using EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using EnergyOptimizer.API.DTOs.Gemini;
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
            var data = new List<EnergyAnalysis> { new EnergyAnalysis { AnalysisType = "Test" } };
            mockRepo.Setup(r => r.ListAllAsync()).ReturnsAsync(data);
            var handler = new GetAnalysisHistoryHandler(mockRepo.Object);
            var query = new GetAnalysisHistoryQuery(1, 10, null, null, null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DetectDeviceAnomalies_CallsPatternService()
        {
            // Arrange
            var mockService = new Mock<IPatternDetectionService>();
            mockService.Setup(s => s.DetectDeviceAnomalies(It.IsAny<int>(), It.IsAny<int>()))
               .ReturnsAsync(new AnomalyDetectionResult
               {
                   Anomalies = new List<DetectedAnomaly>(), 
                   HasAnomalies = false
               });
            var handler = new DetectDeviceAnomaliesHandler(mockService.Object);

            // Act
            var result = await handler.Handle(new DetectDeviceAnomaliesCommand(1, 7), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            mockService.Verify(s => s.DetectDeviceAnomalies(1, 7), Times.Once);
        }
    }
}