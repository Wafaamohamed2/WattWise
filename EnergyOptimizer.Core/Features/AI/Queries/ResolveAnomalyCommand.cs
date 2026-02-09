using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record ResolveAnomalyCommand(int Id, string ResolutionNotes) : IRequest<ApiResponse>;
}
