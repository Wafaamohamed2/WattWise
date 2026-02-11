using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries
{
   public record GetAlertsQuery(
        bool? IsRead,
        int? Severity,
        int? DeviceId,
        string? StartDate,
        string? EndDate,
        int Page,
        int PageSize) : IRequest<ApiResponse>;
}
