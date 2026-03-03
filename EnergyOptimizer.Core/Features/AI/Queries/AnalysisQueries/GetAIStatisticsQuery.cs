using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries
{
    public record GetAIStatisticsQuery() : IRequest<ApiResponse>;
}
