using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record PredictConsumptionQuery(int Days) : IRequest<ApiResponse>;
}
