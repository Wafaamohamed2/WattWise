using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans
{
    public record MarkAlertAsReadCommand (int Id) : IRequest<ApiResponse>;
    
}
