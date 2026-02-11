using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record GetAnomalyByIdQuery(int Id) : IRequest<ApiResponse>;
}
