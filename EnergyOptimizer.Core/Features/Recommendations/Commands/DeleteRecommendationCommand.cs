using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Recommendations.Commands
{
    public record DeleteRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
