using MediatR;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.Reco;
using EnergyOptimizer.Core.Features.AI.Commands;

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
            var recommendations = await _recommendationRepo.ListAllAsync();
            var query = recommendations.AsEnumerable();

            if (request.IsImplemented.HasValue)
                query = query.Where(r => r.IsImplemented == request.IsImplemented.Value);

            var result = query.OrderByDescending(r => r.Priority).ToList();

            return new ApiResponse(200, "Recommendations retrieved successfully", result);
        }
    }
}