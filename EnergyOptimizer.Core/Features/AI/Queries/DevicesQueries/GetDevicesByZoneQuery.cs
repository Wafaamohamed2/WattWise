using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetDevicesByZoneQuery(int ZoneId) : IRequest<ApiResponse>;


}
