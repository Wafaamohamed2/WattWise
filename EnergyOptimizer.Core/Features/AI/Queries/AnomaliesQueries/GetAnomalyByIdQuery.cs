using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record GetAnomalyByIdQuery(int Id) : IRequest<ApiResponse>;
}
