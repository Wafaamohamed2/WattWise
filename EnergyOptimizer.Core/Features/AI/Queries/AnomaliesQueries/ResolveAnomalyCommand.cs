using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record ResolveAnomalyCommand(int Id, string ResolutionNotes) : IRequest<ApiResponse>;
}
