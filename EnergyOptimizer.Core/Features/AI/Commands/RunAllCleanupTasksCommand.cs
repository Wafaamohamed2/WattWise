using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record RunAllCleanupTasksCommand(CancellationToken CancellationToken) : IRequest<ApiResponse>;
}
