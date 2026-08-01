using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Specifications.ZoneSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class CreateDeviceHandler : IRequestHandler<CreateDeviceCommand, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public CreateDeviceHandler(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<Zone> zoneRepo,
            ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CreateDeviceCommand request, CancellationToken ct)
        {
            var zoneSpec = new ZoneOwnedByUserSpec(request.Dto.ZoneId, _currentUser.RequireUserId());
            var zoneExists = await _zoneRepo.AnyAsync(zoneSpec);

            if (!zoneExists)
                throw new NotFoundException($"Zone with ID {request.Dto.ZoneId} not found");

            var device = new Device
            {
                Name = request.Dto.Name,
                ZoneId = request.Dto.ZoneId,
                Type = request.Dto.Type,
                RatedPowerKW = request.Dto.RatedPowerKW,
                IsActive = request.Dto.IsActive,
                InstallationDate = request.Dto.InstallationDate ?? DateTime.UtcNow
            };

            _deviceRepo.Add(device);
            await _deviceRepo.SaveChangesAsync();

            return new ApiResponse(201, "Device created successfully", device);
        }
    }
}
