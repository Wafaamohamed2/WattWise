using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetDevicesByZoneQuery(int ZoneId) : IRequest<ApiResponse>;


}
