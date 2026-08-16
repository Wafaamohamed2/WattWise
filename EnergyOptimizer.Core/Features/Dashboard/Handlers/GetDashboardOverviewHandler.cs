using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Features.Dashboard.Queries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Handlers
{
    public class GetDashboardOverviewHandler : IRequestHandler<GetDashboardOverviewQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDashboardOverviewHandler(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<EnergyReading> readingRepo,
            IGenericRepository<Alert> alertRepo,
            IGenericRepository<Zone> zoneRepo,
            ICurrentUserService currentUser)
        {
            _deviceRepo = deviceRepo;
            _readingRepo = readingRepo;
            _alertRepo = alertRepo;
            _zoneRepo = zoneRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetDashboardOverviewQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var totalDevices = await _deviceRepo.CountAsync(new CountActiveDevicesSpec(userId, isActive: null, buildingId: request.BuildingId));
            var activeDevices = await _deviceRepo.CountAsync(new CountActiveDevicesSpec(userId, isActive: true, buildingId: request.BuildingId));
            var totalZones = await _zoneRepo.CountAsync(new ZoneCountSpec(userId, buildingId: request.BuildingId));

            var latestReadings = await _readingRepo.ListAsync(new LatestReadingsSpec(userId, 10, buildingId: request.BuildingId));
            var currentConsumption = (double)latestReadings.Sum(r => r.PowerConsumptionKW);

            var unreadAlerts = await _alertRepo.CountAsync(new AlertCountSpec(userId, isRead: false, buildingId: request.BuildingId));

            var overview = new
            {
                TotalDevices = totalDevices,
                ActiveDevices = activeDevices,
                TotalZones = totalZones,
                CurrentPowerUsageKW = Math.Round(currentConsumption, 2),
                UnreadAlertsCount = unreadAlerts,
                LastUpdate = DateTime.UtcNow
            };

            return new ApiResponse(200, "Dashboard overview retrieved successfully", overview);
        }
    }
}