using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AnalyzePatternsQuery(DateTime? StartDate, DateTime? EndDate) : IRequest<ApiResponse>;
}
