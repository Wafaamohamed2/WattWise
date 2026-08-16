using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Alerts.Queries
{
    public record GetUnreadAlertsCountQuery(int? BuildingId = null) : IRequest<ApiResponse>;
}
