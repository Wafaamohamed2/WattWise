using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetHourlyConsumptionQuery(string? Date) : IRequest<ApiResponse>;
}
