using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
    public record GetDeviceStatisticsQuery(int DeviceId, DateTime? StartDate, int Days) : IRequest<ApiResponse>;
}
