using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans
{
    public record ImplementRecommendationCommand(int Id) : IRequest<ApiResponse>;
}
