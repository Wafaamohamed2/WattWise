using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Buildings.Commands;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Buildings.Handlers
{
    public class CreateBuildingHandler : IRequestHandler<CreateBuildingCommand, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public CreateBuildingHandler(IGenericRepository<Building> buildingRepo, ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CreateBuildingCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            var existingSpec = new BuildingOwnedByUserSpec(userId);
            if (await _buildingRepo.AnyAsync(existingSpec))
            {
                throw new BadRequestException("You already have a registered building. Please update your existing building instead.");
            }

            var building = new Building
            {
                Name = request.Dto.Name,
                Address = request.Dto.Address,
                TotalArea = request.Dto.TotalArea,
                NumberOfRooms = request.Dto.NumberOfRooms,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _buildingRepo.Add(building);
            await _buildingRepo.SaveChangesAsync();

            return new ApiResponse(201, "Building created successfully", building);
        }
    }
}
