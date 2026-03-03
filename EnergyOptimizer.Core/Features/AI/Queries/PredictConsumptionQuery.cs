using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record PredictConsumptionQuery(int Days) : IRequest<ApiResponse>;
}
