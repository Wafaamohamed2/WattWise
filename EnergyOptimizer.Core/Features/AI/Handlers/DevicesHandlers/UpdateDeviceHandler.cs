using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class UpdateDeviceHandler : IRequestHandler<UpdateDeviceCommand, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public UpdateDeviceHandler(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<Zone> zoneRepo,
            ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(UpdateDeviceCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new DeviceWithDetailsSpec(request.id, userId);
            var device = await _deviceRepo.GetEntityWithSpec(spec);

            if (device == null)
                throw new NotFoundException($"Device with ID {request.id} not found");

            if (request.Dto.ZoneId.HasValue && request.Dto.ZoneId.Value != device.ZoneId)
            {
                var zoneSpec = new ZoneOwnedByUserSpec(request.Dto.ZoneId.Value, userId);
                var zoneExists = await _zoneRepo.AnyAsync(zoneSpec);
                if (!zoneExists)
                    throw new NotFoundException($"Zone with ID {request.Dto.ZoneId} not found");

                device.ZoneId = request.Dto.ZoneId.Value;
            }

            device.Name = request.Dto.Name;
            if (request.Dto.Type.HasValue) device.Type = request.Dto.Type.Value;
            if (request.Dto.RatedPowerKW.HasValue) device.RatedPowerKW = request.Dto.RatedPowerKW.Value;
            if (request.Dto.IsActive.HasValue) device.IsActive = request.Dto.IsActive.Value;

            _deviceRepo.Update(device);
            await _deviceRepo.SaveChangesAsync();

            return new ApiResponse(200, "Device updated successfully", device);
        }
    }
}
