using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record ResolveAnomalyCommand(int Id, string ResolutionNotes) : IRequest<ApiResponse>;
}
