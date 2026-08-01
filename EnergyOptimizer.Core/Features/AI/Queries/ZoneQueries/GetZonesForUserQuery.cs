using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.ZoneQueries
{
    public record GetZonesForUserQuery(int? BuildingId = null) : IRequest<ApiResponse>;
}
