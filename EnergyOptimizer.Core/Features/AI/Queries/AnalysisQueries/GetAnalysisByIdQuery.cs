using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries
{
    public record GetAnalysisByIdQuery(int Id) : IRequest<ApiResponse>;
}
