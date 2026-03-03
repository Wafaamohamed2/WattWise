using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetDashboardOverviewHandler : IRequestHandler<GetDashboardOverviewQuery, ApiResponse>
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;

        public GetDashboardOverviewHandler(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<EnergyReading> readingRepo,
            IGenericRepository<Alert> alertRepo,
            IGenericRepository<Zone> zoneRepo)
        {
            _deviceRepo = deviceRepo;
            _readingRepo = readingRepo;
            _alertRepo = alertRepo;
            _zoneRepo = zoneRepo;
        }

        public async Task<ApiResponse> Handle(GetDashboardOverviewQuery request, CancellationToken ct)
        {
            var totalDevices = await _deviceRepo.CountAsync(new CountActiveDevicesSpec());
            var activeDevices = await _deviceRepo.CountAsync(new CountActiveDevicesSpec(true));
            var totalZones = await _zoneRepo.CountAsync(new ZoneCountSpec());

            var latestReadings = await _readingRepo.ListAsync(new LatestReadingsSpec(10));
            var currentConsumption = (double)latestReadings.Sum(r => r.PowerConsumptionKW);

            var unreadAlerts = await _alertRepo.CountAsync(new AlertCountSpec(isRead: false));

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