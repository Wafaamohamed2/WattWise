using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Devices.Queries
{
   public record GetDevicesByZoneQuery(int ZoneId) : IRequest<ApiResponse>;


}
