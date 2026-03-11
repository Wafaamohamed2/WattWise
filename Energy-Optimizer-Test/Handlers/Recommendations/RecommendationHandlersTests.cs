using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers;
using EnergyOptimizer.Core.Features.AI.Queries.Reco;

namespace EnergyOptimizer.Tests.Handlers.Recommendations
{
    public class RecommendationHandlersTests
    {
        private readonly Mock<IGenericRepository<EnergyRecommendation>> _mockRepo;

        public RecommendationHandlersTests()
        {
            _mockRepo = new Mock<IGenericRepository<EnergyRecommendation>>();
        }

        [Fact]
        public async Task GetRecommendations_FiltersCorrectly()
        {
            // Arrange
            var data = new List<EnergyRecommendation> {
                new EnergyRecommendation { Id = 1, IsImplemented = true },
                new EnergyRecommendation { Id = 2, IsImplemented = false }
            };
            _mockRepo.Setup(r => r.ListAllAsync()).ReturnsAsync(data);
            var handler = new GetRecommendationsHandler(_mockRepo.Object);

            // Act
            var query = new GetRecommendationsQuery(IsImplemented: false);
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            var details = result.Details as List<EnergyRecommendation>;
            details!.Count.Should().Be(1);
            details![0].Id.Should().Be(2);
        }

        [Fact]
        public async Task DeleteRecommendation_CallsDelete()
        {
            // Arrange
            var rec = new EnergyRecommendation { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rec);
            var handler = new DeleteRecommendationHandler(_mockRepo.Object);

            // Act
            var result = await handler.Handle(new DeleteRecommendationCommand(1), CancellationToken.None);

            // Assert
            _mockRepo.Verify(r => r.DeleteAsync(rec), Times.Once);
            result.StatusCode.Should().Be(200);
        }
    }
}