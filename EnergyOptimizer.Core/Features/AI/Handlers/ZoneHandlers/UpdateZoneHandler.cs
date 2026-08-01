using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.ZoneCommands;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ZoneHandlers
{
    public class UpdateZoneHandler : IRequestHandler<UpdateZoneCommand, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public UpdateZoneHandler(IGenericRepository<Zone> zoneRepo, ICurrentUserService currentUser)
        {
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(UpdateZoneCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new ZoneOwnedByUserSpec(request.Id, userId);
            var zone = await _zoneRepo.GetEntityWithSpec(spec);

            if (zone == null)
                throw new NotFoundException($"Zone with ID {request.Id} not found");

            zone.Name = request.Dto.Name;
            if (request.Dto.Type.HasValue) zone.Type = request.Dto.Type.Value;
            if (request.Dto.Area.HasValue) zone.Area = request.Dto.Area.Value;

            _zoneRepo.Update(zone);
            await _zoneRepo.SaveChangesAsync();

            return new ApiResponse(200, "Zone updated successfully", zone);
        }
    }
}
