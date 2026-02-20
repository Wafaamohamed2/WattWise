using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetAllDevicesHandler : IRequestHandler<GetAllDevicesQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;

        public GetAllDevicesHandler(IGenericRepository<Device> deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        public async Task<ApiResponse> Handle(GetAllDevicesQuery request, CancellationToken ct)
        {
            var spec = new ActiveDevicesWithZoneSpec(request.IsActive);
            var devices = await _deviceRepo.ListAsync(spec);

            return new ApiResponse(200, "Devices retrieved successfully", devices);
        }
    }
}