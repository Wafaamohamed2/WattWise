using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record DeleteRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
