using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.BuildingCommands;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.BuildingHandlers
{
    public class UpdateBuildingHandler : IRequestHandler<UpdateBuildingCommand, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public UpdateBuildingHandler(IGenericRepository<Building> buildingRepo, ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(UpdateBuildingCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new BuildingOwnedByUserSpec(request.Id, userId);
            var building = await _buildingRepo.GetEntityWithSpec(spec);

            if (building == null)
                throw new NotFoundException($"Building with ID {request.Id} not found");

            building.Name = request.Dto.Name;
            building.Address = request.Dto.Address;
            building.TotalArea = request.Dto.TotalArea;
            building.NumberOfRooms = request.Dto.NumberOfRooms;

            _buildingRepo.Update(building);
            await _buildingRepo.SaveChangesAsync();

            return new ApiResponse(200, "Building updated successfully", building);
        }
    }
}
