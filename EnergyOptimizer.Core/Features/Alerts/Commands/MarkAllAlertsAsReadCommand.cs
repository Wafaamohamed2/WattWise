using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Alerts.Commands
{
   public record MarkAllAlertsAsReadCommand(int? BuildingId = null) : IRequest<ApiResponse>;
}
