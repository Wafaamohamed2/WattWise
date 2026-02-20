using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

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
