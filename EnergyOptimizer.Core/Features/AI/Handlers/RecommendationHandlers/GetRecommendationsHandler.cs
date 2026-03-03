using MediatR;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.Reco;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Specifications.RecommendationSpec;

namespace EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers
{
    public class GetRecommendationsHandler : IRequestHandler<GetRecommendationsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;

        public GetRecommendationsHandler(IGenericRepository<EnergyRecommendation> recommendationRepo)
        {
            _recommendationRepo = recommendationRepo;
        }

        public async Task<ApiResponse> Handle(GetRecommendationsQuery request, CancellationToken ct)
        {
            var spec = new RecommendationsFilterSpec(request.IsImplemented);
            var recommendations = await _recommendationRepo.ListAsync(spec);

            return new ApiResponse(200, "Recommendations retrieved successfully", recommendations);
        }
    }
}