using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
   public record GetDeviceStatisticsQuery(int DeviceId, DateTime StartDate, DateTime EndDate) : IRequest<ApiResponse>;
     
}
