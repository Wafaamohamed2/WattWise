using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Zones.Queries
{
    public record GetZonesForUserQuery(int? BuildingId = null) : IRequest<ApiResponse>;
}
