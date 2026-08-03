using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Zones.Commands;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Zones.Handlers
{
    public class DeleteZoneHandler : IRequestHandler<DeleteZoneCommand, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public DeleteZoneHandler(IGenericRepository<Zone> zoneRepo, ICurrentUserService currentUser)
        {
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeleteZoneCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new ZoneOwnedByUserSpec(request.ZoneId, userId);
            var zone = await _zoneRepo.GetEntityWithSpec(spec);

            if (zone == null)
                throw new NotFoundException($"Zone with ID {request.ZoneId} not found");

            if (zone.Devices != null && zone.Devices.Any())
            {
                throw new BadRequestException("Cannot delete a zone that contains active devices. Please remove or relocate the devices first.");
            }

            _zoneRepo.Delete(zone);
            await _zoneRepo.SaveChangesAsync();

            return new ApiResponse(200, "Zone deleted successfully");
        }
    }
}
