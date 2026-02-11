using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
    public record GetDeviceReadingsQuery(int DeviceId, string? StartDate, string? EndDate, int Limit) : IRequest<ApiResponse>;
}
