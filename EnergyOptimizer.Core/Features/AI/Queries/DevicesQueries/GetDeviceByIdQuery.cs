using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetDeviceByIdQuery(int Id) : IRequest<ApiResponse>;
}
