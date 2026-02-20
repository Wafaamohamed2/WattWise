using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans
{
    public record ImplementRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
