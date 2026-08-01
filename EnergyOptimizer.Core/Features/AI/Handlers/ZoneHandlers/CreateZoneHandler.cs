using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.ZoneCommands;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ZoneHandlers
{
    public class CreateZoneHandler : IRequestHandler<CreateZoneCommand, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public CreateZoneHandler(
            IGenericRepository<Zone> zoneRepo,
            IGenericRepository<Building> buildingRepo,
            ICurrentUserService currentUser)
        {
            _zoneRepo = zoneRepo;
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CreateZoneCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var buildingSpec = new BuildingOwnedByUserSpec(request.Dto.BuildingId, userId);
            var building = await _buildingRepo.GetEntityWithSpec(buildingSpec);

            if (building == null)
                throw new NotFoundException($"Building with ID {request.Dto.BuildingId} not found");

            var zone = new Zone
            {
                Name = request.Dto.Name,
                BuildingId = request.Dto.BuildingId,
                Type = request.Dto.Type,
                Area = request.Dto.Area
            };

            _zoneRepo.Add(zone);
            await _zoneRepo.SaveChangesAsync();

            return new ApiResponse(201, "Zone created successfully", zone);
        }
    }
}
