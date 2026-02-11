using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
   public record GetLatestReadingsQuery(int Limit) :IRequest<ApiResponse>;
}
