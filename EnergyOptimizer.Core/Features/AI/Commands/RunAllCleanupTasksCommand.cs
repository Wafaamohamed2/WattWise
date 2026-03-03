using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record RunAllCleanupTasksCommand: IRequest<ApiResponse>;
}
