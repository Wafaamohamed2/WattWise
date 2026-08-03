using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Recommendations.Commands
{
    public record ImplementRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
