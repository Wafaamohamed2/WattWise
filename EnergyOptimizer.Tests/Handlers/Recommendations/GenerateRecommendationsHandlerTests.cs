using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Tests.Handlers.Recommendations
{
    public class GenerateRecommendationsHandlerTests
    {
        private readonly Mock<IPatternDetectionService> _mockPattern;
        private readonly GenerateRecommendationsHandler _handler;

        public GenerateRecommendationsHandlerTests()
        {
            _mockPattern = new Mock<IPatternDetectionService>();
            _handler = new GenerateRecommendationsHandler(
                _mockPattern.Object,
                new Mock<IGenericRepository<EnergyAnalysis>>().Object,
                new Mock<IGenericRepository<EnergyRecommendation>>().Object);
        }

        [Fact]
        public async Task Handle_InvalidDateRange_ThrowsBadRequest()
        {
            // Arrange
            var command = new GenerateRecommendationsCommand(DateTime.UtcNow, DateTime.UtcNow.AddDays(-1));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}
