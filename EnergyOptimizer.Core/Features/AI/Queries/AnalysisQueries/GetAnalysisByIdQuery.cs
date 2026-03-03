using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries
{
    public record GetAnalysisByIdQuery(int Id) : IRequest<ApiResponse>;
}
