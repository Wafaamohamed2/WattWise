using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetAllDevicesQuery(bool? IsActive, int? ZoneId, DeviceType? DeviceType, decimal? MinPower, decimal? MaxPower, int Page, int PageSize) : IRequest<ApiResponse>;
}
