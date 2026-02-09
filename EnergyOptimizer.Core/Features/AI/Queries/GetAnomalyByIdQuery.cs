using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record GetAnomalyByIdQuery(int Id) : IRequest<ApiResponse>;
}
