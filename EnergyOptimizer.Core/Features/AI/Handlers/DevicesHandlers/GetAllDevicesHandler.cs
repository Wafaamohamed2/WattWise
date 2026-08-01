using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetAllDevicesHandler : IRequestHandler<GetAllDevicesQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public GetAllDevicesHandler(IGenericRepository<Device> deviceRepo, ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAllDevicesQuery request, CancellationToken ct)
        {
            var spec = new ActiveDevicesWithZoneSpec(request.IsActive, _currentUser.RequireUserId());
            var devices = await _deviceRepo.ListAsync(spec);

            var deviceDtos = devices.Select(d => new
            {
                id = d.Id,
                name = d.Name,
                type = d.Type.ToString(),
                ratedPowerKW = d.RatedPowerKW,
                isActive = d.IsActive,
                installationDate = d.InstallationDate,
                zoneId = d.ZoneId,
                zoneName = d.Zone?.Name ?? "Unknown Zone",
                zone = d.Zone != null ? new { id = d.Zone.Id, name = d.Zone.Name } : null
            }).ToList();

            return new ApiResponse(200, "Devices retrieved successfully", deviceDtos);
        }
    }
}