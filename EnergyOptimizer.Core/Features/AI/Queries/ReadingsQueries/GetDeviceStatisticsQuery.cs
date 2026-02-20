using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
    public record GetDeviceStatisticsQuery(int DeviceId, DateTime? StartDate, int Days) : IRequest<ApiResponse>;
}
