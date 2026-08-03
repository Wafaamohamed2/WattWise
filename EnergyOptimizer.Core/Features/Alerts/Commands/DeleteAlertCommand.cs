using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Alerts.Commands
{
    public record DeleteAlertCommand (int Id) : IRequest<ApiResponse>;
    
}
