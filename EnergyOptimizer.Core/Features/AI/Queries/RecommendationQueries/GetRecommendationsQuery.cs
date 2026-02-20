using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.Reco
{
    public record GetRecommendationsQuery(bool? IsImplemented) : IRequest<ApiResponse>;
}
