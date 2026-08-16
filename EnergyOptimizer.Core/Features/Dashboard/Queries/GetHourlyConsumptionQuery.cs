using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetHourlyConsumptionQuery(string? Date, int? BuildingId = null) : IRequest<ApiResponse>;
}
