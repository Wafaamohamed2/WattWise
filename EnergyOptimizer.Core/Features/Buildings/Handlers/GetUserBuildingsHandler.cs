using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.Buildings.Queries;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Buildings.Handlers
{
    public class GetUserBuildingsHandler : IRequestHandler<GetUserBuildingsQuery, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetUserBuildingsHandler(IGenericRepository<Building> buildingRepo, ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetUserBuildingsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new BuildingOwnedByUserSpec(userId);
            var buildings = await _buildingRepo.ListAsync(spec);

            return new ApiResponse(200, "Buildings retrieved successfully", buildings);
        }
    }
}
