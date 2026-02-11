using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class ToggleDeviceHandler : IRequestHandler<ToggleDeviceCommand, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IEnergyHubService _hubService;

        public ToggleDeviceHandler(IGenericRepository<Device> deviceRepo, IEnergyHubService hubService)
        {
            _deviceRepo = deviceRepo;
            _hubService = hubService;
        }

        public async Task<ApiResponse> Handle(ToggleDeviceCommand request, CancellationToken ct)
        {
            var spec = new DeviceWithDetailsSpec(request.Id);
            var device = await _deviceRepo.GetEntityWithSpec(spec);

            if (device == null) return new ApiResponse(404, "Device not found");

            device.IsActive = !device.IsActive;
            _deviceRepo.Update(device);
            await _deviceRepo.SaveChangesAsync();

            await _hubService.NotifyDeviceStatusChanged(device.Id, device.IsActive);

            return new ApiResponse(200, $"Device {(device.IsActive ? "activated" : "deactivated")}");
        }
    }
}
