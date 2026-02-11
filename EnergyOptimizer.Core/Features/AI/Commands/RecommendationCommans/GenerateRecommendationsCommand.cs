using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans
{
    public record GenerateRecommendationsCommand(DateTime? StartDate, DateTime? EndDate) : IRequest<ApiResponse>;
}
