using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries.Reco
{
    public record GetRecommendationsQuery(bool? IsImplemented) : IRequest<ApiResponse>;
}
