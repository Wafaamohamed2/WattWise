using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries
{
   public record GetAlertsQuery(
        bool? IsRead,
        int? Severity,
        int? DeviceId,
        string? StartDate,
        string? EndDate,
        int Page= 1,
        int PageSize= 20) : IRequest<ApiResponse>;
}
