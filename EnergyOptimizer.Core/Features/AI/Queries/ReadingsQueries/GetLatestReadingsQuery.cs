using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
   public record GetLatestReadingsQuery(int Limit) :IRequest<ApiResponse>;
}
