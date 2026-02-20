using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetDeviceByIdQuery(int Id) : IRequest<ApiResponse>;
}
