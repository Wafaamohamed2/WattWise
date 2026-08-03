using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Alerts.Commands
{
    public record MarkAlertAsReadCommand (int Id) : IRequest<ApiResponse>;
    
}
