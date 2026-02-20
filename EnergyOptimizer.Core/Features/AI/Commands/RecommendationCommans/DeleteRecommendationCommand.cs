using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans
{
    public record DeleteRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
