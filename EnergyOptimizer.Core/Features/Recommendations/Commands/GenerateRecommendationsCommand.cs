using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Recommendations.Commands
{
    public record GenerateRecommendationsCommand(DateTime? StartDate, DateTime? EndDate) : IRequest<ApiResponse>;
}
