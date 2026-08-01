using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.BuildingQueries;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.BuildingHandlers
{
    public class GetUserBuildingHandler : IRequestHandler<GetUserBuildingQuery, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetUserBuildingHandler(IGenericRepository<Building> buildingRepo, ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetUserBuildingQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new BuildingOwnedByUserSpec(userId);
            var building = await _buildingRepo.GetEntityWithSpec(spec);

            if (building == null)
                throw new NotFoundException("No building found for the current user.");

            return new ApiResponse(200, "Building retrieved successfully", building);
        }
    }
}
