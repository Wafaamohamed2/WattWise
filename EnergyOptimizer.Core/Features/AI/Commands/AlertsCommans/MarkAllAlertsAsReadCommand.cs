using MediatR;


namespace EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans
{
   public record MarkAllAlertsAsReadCommand : IRequest<ApiResponse>;
    
}
