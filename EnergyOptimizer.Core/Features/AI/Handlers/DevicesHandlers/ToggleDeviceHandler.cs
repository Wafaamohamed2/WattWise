using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions; 
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

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

            if (device == null)
                throw new NotFoundException($"Device with ID {request.Id} not found");

            device.IsActive = !device.IsActive;
            _deviceRepo.Update(device);
            await _deviceRepo.SaveChangesAsync();

            await _hubService.NotifyDeviceStatusChanged(device.Id, device.IsActive);

            return new ApiResponse(200, $"Device {(device.IsActive ? "activated" : "deactivated")}");
        }
    }
}