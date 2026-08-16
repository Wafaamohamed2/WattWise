using EnergyOptimizer.Core.Enums;
using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Devices.Queries
{
   public record GetAllDevicesQuery(
       bool? IsActive, 
       int? ZoneId, 
       DeviceType? DeviceType, 
       decimal? MinPower, 
       decimal? MaxPower, 
       int Page = 1, 
       int PageSize = 50, 
       int? BuildingId = null
   ) : IRequest<ApiResponse>;
}
