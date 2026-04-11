using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using EnergyOptimizer.API.DTOs.Gemini;

namespace EnergyOptimizer.Tests.Handlers.Analyze
{
    public class AnalyzePatternsHandlerTests
    {
        [Fact]
        public async Task Handle_ValidRequest_ReturnsAnalysisResult()
        {
            // Arrange
            var mockPatternService = new Mock<IPatternDetectionService>();
            var mockAnalysisRepo = new Mock<IGenericRepository<EnergyAnalysis>>();

            var expectedResult = new GeminiAnalysisResult
            {
                Success = true,
                Summary = "Test summary",
                Insights = new List<string> { "Insight 1" } 
            };

            mockPatternService
                .Setup(s => s.AnalyzeConsumptionPatterns(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(expectedResult);

            mockAnalysisRepo
               .Setup(r => r.Add(It.IsAny<EnergyAnalysis>()));

            mockAnalysisRepo
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);  

            var handler = new AnalyzePatternsHandler(mockPatternService.Object, mockAnalysisRepo.Object);

            // Act
            var result = await handler.Handle(
                new AnalyzePatternsQuery(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
        }
    }
}