using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetConsumptionByDeviceQuery(string? StartDate, string? EndDate):IRequest<ApiResponse>;
}
