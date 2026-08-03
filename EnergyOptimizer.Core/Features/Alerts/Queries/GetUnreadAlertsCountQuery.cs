using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Alerts.Queries
{
   public record GetUnreadAlertsCountQuery : IRequest<ApiResponse>;


}
