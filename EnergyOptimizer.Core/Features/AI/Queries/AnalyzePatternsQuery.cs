using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AnalyzePatternsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<ApiResponse>;
}
