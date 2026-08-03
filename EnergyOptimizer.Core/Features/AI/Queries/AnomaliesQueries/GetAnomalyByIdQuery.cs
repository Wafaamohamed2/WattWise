using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record GetAnomalyByIdQuery(int Id) : IRequest<ApiResponse>;
}
