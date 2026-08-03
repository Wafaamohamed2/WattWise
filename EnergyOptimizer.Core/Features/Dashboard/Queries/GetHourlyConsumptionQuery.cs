using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetHourlyConsumptionQuery(string? Date) : IRequest<ApiResponse>;
}
