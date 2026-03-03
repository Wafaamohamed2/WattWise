using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans
{
    public record ClearReadAlertsCommand : IRequest<ApiResponse>;
   
}
