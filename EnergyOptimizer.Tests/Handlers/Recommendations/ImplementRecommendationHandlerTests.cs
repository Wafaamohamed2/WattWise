using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;

namespace EnergyOptimizer.Tests.Handlers.Recommendations
{
    public class ImplementRecommendationHandlerTests
    {
        private readonly Mock<IGenericRepository<EnergyRecommendation>> _mockRepo;
        private readonly ImplementRecommendationHandler _handler;

        public ImplementRecommendationHandlerTests()
        {
            _mockRepo = new Mock<IGenericRepository<EnergyRecommendation>>();
            _handler = new ImplementRecommendationHandler(_mockRepo.Object);
        }

        [Fact]
        public async Task Handle_ValidId_MarksAsImplemented()
        {
            // Arrange
            var rec = new EnergyRecommendation { Id = 1, IsImplemented = false };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rec);

            // Act
            var result = await _handler.Handle(new ImplementRecommendationCommand(1), CancellationToken.None);

            // Assert
            rec.IsImplemented.Should().BeTrue();
            rec.ImplementedDate.Should().NotBeNull();
            result.Message.Should().Be("Marked as implemented");
        }
    }
}
