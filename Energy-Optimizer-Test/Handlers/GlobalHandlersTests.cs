using EnergyOptimizer.Core.Features.AI.Handlers;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Commands;
using FluentAssertions;
using Moq;
using EnergyOptimizer.Service.Services.Abstract;

namespace EnergyOptimizer.Tests.Handlers
{
    public class GlobalHandlersTests
    {
        [Fact]
        public async Task PredictConsumption_ReturnsPrediction()
        {
            // Arrange
            var mockPatternService = new Mock<IPatternDetectionService>();
            var expectedResult = new EnergyOptimizer.API.DTOs.Gemini.PredictionResult
            {
                PredictedConsumptionKWh = 150.5,
                ConfidenceScore = 0.9,
                PredictionDate = DateTime.UtcNow.AddDays(7),
                Explanation = "Test prediction"
            };

            mockPatternService
                .Setup(s => s.PredictConsumption(It.IsAny<int>()))
                .ReturnsAsync(expectedResult);
            var handler = new PredictConsumptionHandler(mockPatternService.Object);

            // Act
            var result = await handler.Handle(new PredictConsumptionQuery(7), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task RunGlobalAnalysis_CallsService()
        {
            // Arrange
            var mockService = new Mock<IAIAnalysisService>();
            var handler = new RunGlobalAnalysisHandler(mockService.Object);

            // Act
            var result = await handler.Handle(new RunGlobalAnalysisCommand(), CancellationToken.None);

            // Assert
            mockService.Verify(s => s.RunGlobalAnalysisAsync(It.IsAny<CancellationToken>()), Times.Once);

            result.Message.Should().Contain("successfully");
        }
    }
}