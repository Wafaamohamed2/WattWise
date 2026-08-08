using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Recommendations.Queries
{
    public record GetRecommendationsQuery(bool? IsImplemented) : IRequest<ApiResponse>;
}
