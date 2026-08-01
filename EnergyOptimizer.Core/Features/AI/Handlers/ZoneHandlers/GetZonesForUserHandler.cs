using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.ZoneQueries;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ZoneHandlers
{
    public class GetZonesForUserHandler : IRequestHandler<GetZonesForUserQuery, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public GetZonesForUserHandler(IGenericRepository<Zone> zoneRepo, ICurrentUserService currentUser)
        {
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetZonesForUserQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = request.BuildingId.HasValue
                ? new ZonesForUserBuildingSpec(request.BuildingId.Value, userId)
                : new ZonesForUserBuildingSpec(userId);

            var zones = await _zoneRepo.ListAsync(spec);

            return new ApiResponse(200, "Zones retrieved successfully", zones);
        }
    }
}
