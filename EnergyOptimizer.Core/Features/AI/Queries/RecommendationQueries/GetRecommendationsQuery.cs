using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.Reco
{
    public record GetRecommendationsQuery(bool? IsImplemented) : IRequest<ApiResponse>;
}
