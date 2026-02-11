using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans
{
    public record DeleteRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
