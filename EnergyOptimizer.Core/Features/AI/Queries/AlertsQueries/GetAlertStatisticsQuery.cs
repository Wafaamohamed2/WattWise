using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries
{
    public record GetAlertStatisticsQuery(string? StartDate, int Days) : IRequest<ApiResponse>;



}
