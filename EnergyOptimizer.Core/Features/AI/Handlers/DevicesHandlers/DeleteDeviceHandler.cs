using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class DeleteDeviceHandler : IRequestHandler<DeleteDeviceCommand, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public DeleteDeviceHandler(IGenericRepository<Device> deviceRepo, ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeleteDeviceCommand request, CancellationToken ct)
        {
            var spec = new DeviceWithDetailsSpec(request.DeviceId, _currentUser.RequireUserId());
            var device = await _deviceRepo.GetEntityWithSpec(spec);

            if (device == null)
                throw new NotFoundException($"Device with ID {request.DeviceId} not found");

            _deviceRepo.Delete(device);
            await _deviceRepo.SaveChangesAsync();

            return new ApiResponse(200, "Device deleted successfully");
        }
    }
}
