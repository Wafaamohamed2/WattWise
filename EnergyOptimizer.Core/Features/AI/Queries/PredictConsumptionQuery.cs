using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record PredictConsumptionQuery(int Days) : IRequest<ApiResponse>;
}
