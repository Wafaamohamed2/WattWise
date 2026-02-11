using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetHourlyConsumptionQuery(string? Date) : IRequest<ApiResponse>;
}
