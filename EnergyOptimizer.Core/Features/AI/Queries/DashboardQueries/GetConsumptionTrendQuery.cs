using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetConsumptionTrendQuery(int Hours): IRequest<ApiResponse>;
}
