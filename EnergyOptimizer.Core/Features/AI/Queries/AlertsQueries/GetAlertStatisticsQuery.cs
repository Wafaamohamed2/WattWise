using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries
{
    public record GetAlertStatisticsQuery(string? StartDate, int Days) : IRequest<ApiResponse>;



}
