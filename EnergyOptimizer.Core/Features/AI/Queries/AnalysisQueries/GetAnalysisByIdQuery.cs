using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries
{
    public record GetAnalysisByIdQuery(int Id) : IRequest<ApiResponse>;
}
