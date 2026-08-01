using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetDeviceByIdHandler : IRequestHandler<GetDeviceByIdQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDeviceByIdHandler(IGenericRepository<Device> deviceRepo, ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetDeviceByIdQuery request, CancellationToken ct)
        {
            var spec = new DeviceWithDetailsSpec(request.Id, _currentUser.RequireUserId());
            var device = await _deviceRepo.GetEntityWithSpec(spec);

            if (device == null)
                throw new NotFoundException($"Device with ID {request.Id} not found");

            return new ApiResponse(200, "Device retrieved successfully", device);
        }
    }
}
