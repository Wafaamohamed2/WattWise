using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.Dashboard.Queries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Handlers
{
    public class GetConsumptionByDeviceHandler : IRequestHandler<GetConsumptionByDeviceQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetConsumptionByDeviceHandler(IGenericRepository<EnergyReading> readingRepo, ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetConsumptionByDeviceQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            if (!DateTime.TryParse(request.StartDate, out var start)) start = DateTime.UtcNow.Date;
            if (!DateTime.TryParse(request.EndDate, out var end)) end = DateTime.UtcNow;

            var spec = new DeviceConsumptionSpec(start, end, userId, request.BuildingId);
            var readings = await _readingRepo.ListAsync(spec);

            var deviceStats = readings
                .GroupBy(r => new { r.DeviceId, DeviceName = r.Device?.Name ?? "Unknown", DeviceType = r.Device?.Type.ToString() ?? "Unknown", ZoneName = r.Device?.Zone?.Name ?? "Unknown" })
                .Select(g => new
                {
                    deviceId = g.Key.DeviceId,
                    deviceName = g.Key.DeviceName,
                    deviceType = g.Key.DeviceType,
                    zoneName = g.Key.ZoneName,
                    totalConsumptionKWh = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                    avgConsumptionKW = Math.Round(g.Average(r => r.PowerConsumptionKW), 2),
                    readingsCount = g.Count()
                })
                .OrderByDescending(x => x.totalConsumptionKWh)
                .ToList();

            return new ApiResponse(200, "Device consumption statistics retrieved", deviceStats);
        }
    }
}
