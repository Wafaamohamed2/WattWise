using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetDevicesByZoneHandler : IRequestHandler<GetDevicesByZoneQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDevicesByZoneHandler(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<Zone> zoneRepo,
            ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetDevicesByZoneQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var zoneSpec = new ZoneOwnedByUserSpec(request.ZoneId, userId);
            if (!await _zoneRepo.AnyAsync(zoneSpec))
                throw new NotFoundException($"Zone with ID {request.ZoneId} not found");

            var spec = new DevicesByZoneSpec(request.ZoneId, userId);
            var devices = await _deviceRepo.ListAsync(spec);

            return new ApiResponse(200, "Devices retrieved successfully", devices);
        }
    }
}
