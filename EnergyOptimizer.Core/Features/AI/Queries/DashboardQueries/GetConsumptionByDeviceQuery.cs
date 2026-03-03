using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetConsumptionByDeviceQuery(string? StartDate, string? EndDate):IRequest<ApiResponse>;
}
