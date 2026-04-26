using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers;
using EnergyOptimizer.Core.Features.AI.Queries.Reco;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.RecommendationSpec;
using FluentAssertions;
using Moq;

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
        public async Task GetRecommendations_WithIsImplementedFalse_ReturnsOnlyActiveRecs()
        {
            // Arrange
            // The spec filters by IsImplemented, so we return only the matching subset
            var activeRecs = new List<EnergyRecommendation>
            {
                new EnergyRecommendation { Id = 2, IsImplemented = false }
            };

            _mockRepo
                .Setup(r => r.ListAsync(It.IsAny<RecommendationsFilterSpec>()))
                .ReturnsAsync(activeRecs);

            var handler = new GetRecommendationsHandler(_mockRepo.Object);

            // Act
            var result = await handler.Handle(
                new GetRecommendationsQuery(IsImplemented: false),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);

            var details = result.Details as IReadOnlyList<EnergyRecommendation>;
            details.Should().NotBeNull();
            details!.Count.Should().Be(1);
            details[0].Id.Should().Be(2);
            details[0].IsImplemented.Should().BeFalse();
        }

        [Fact]
        public async Task GetRecommendations_WithNullFilter_ReturnsAll()
        {
            // Arrange
            var allRecs = new List<EnergyRecommendation>
            {
                new EnergyRecommendation { Id = 1, IsImplemented = true  },
                new EnergyRecommendation { Id = 2, IsImplemented = false }
            };

            _mockRepo
                .Setup(r => r.ListAsync(It.IsAny<RecommendationsFilterSpec>()))
                .ReturnsAsync(allRecs);

            var handler = new GetRecommendationsHandler(_mockRepo.Object);

            // Act
            var result = await handler.Handle(
                new GetRecommendationsQuery(IsImplemented: null),
                CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var details = result.Details as IReadOnlyList<EnergyRecommendation>;
            details!.Count.Should().Be(2);
        }

        [Fact]
        public async Task DeleteRecommendation_ExistingId_CallsDeleteAndSave()
        {
            // Arrange
            var rec = new EnergyRecommendation { Id = 1 };
            _mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(rec);

            var handler = new DeleteRecommendationHandler(_mockRepo.Object);

            // Act
            var result = await handler.Handle(
                new DeleteRecommendationCommand(1),
                CancellationToken.None);

            // Assert 
            _mockRepo.Verify(r => r.Delete(rec), Times.Once);
            _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
            result.StatusCode.Should().Be(200);
        }

        [Fact]
        public async Task DeleteRecommendation_NonExistingId_ThrowsNotFoundException()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((EnergyRecommendation?)null);

            var handler = new DeleteRecommendationHandler(_mockRepo.Object);

            // Act
            Func<Task> act = async () =>
                await handler.Handle(new DeleteRecommendationCommand(99), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EnergyOptimizer.Core.Exceptions.NotFoundException>();
        }
    }
}
