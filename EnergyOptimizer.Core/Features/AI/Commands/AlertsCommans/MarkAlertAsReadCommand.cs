using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans
{
    public record MarkAlertAsReadCommand (int Id) : IRequest<ApiResponse>;
    
}
