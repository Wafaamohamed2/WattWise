using EnergyOptimizer.Core.Enums;
using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Devices.Queries
{
   public record GetAllDevicesQuery(bool? IsActive, int? ZoneId, DeviceType? DeviceType, decimal? MinPower, decimal? MaxPower, int Page, int PageSize) : IRequest<ApiResponse>;
}
