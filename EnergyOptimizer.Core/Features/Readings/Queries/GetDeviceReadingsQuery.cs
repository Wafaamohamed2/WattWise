using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Readings.Queries
{
    public record GetDeviceReadingsQuery(int DeviceId, string? StartDate, string? EndDate, int Limit) : IRequest<ApiResponse>;
}
