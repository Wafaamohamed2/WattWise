using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Alerts.Queries
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
