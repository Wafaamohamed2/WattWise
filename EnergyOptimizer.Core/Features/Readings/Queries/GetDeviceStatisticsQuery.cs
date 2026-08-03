using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Readings.Queries
{
    public record GetDeviceStatisticsQuery(int DeviceId, DateTime? StartDate, int Days) : IRequest<ApiResponse>;
}
