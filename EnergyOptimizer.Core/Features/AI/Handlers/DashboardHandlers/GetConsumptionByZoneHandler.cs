using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Specifications.ReadSpec;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetConsumptionByZoneHandler : IRequestHandler<GetConsumptionByZoneQuery, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;

        public GetConsumptionByZoneHandler(IGenericRepository<Zone> zoneRepo, IGenericRepository<EnergyReading> repository )
        {
            _zoneRepo = zoneRepo;
            _readingRepo = repository;
        }

        public async Task<ApiResponse> Handle(GetConsumptionByZoneQuery request, CancellationToken ct)
        {
            if (!DateTime.TryParse(request.StartDate, out var start)) start = DateTime.UtcNow.Date;
            if (!DateTime.TryParse(request.EndDate, out var end)) end = DateTime.UtcNow;

            var zones = await _zoneRepo.ListAsync(new ZonesWithConsumptionSpec());
            var zoneConsumption = new List<object>();

            foreach (var zone in zones)
            {
                var readings = await _readingRepo.ListAsync(
                    new ReadingsByZoneAndDateSpec(zone.Id, start, end));

                var totalKWh = readings.Sum(r => r.PowerConsumptionKW);
                var activeDevices = zone.Devices?.Count(d => d.IsActive) ?? 0;

                zoneConsumption.Add(new
                {
                    zoneId = zone.Id,
                    zoneName = zone.Name,
                    zoneType = zone.Type.ToString(),
                    totalConsumptionKWh = Math.Round(totalKWh, 2),
                    readingsCount = readings.Count,
                    activeDevices,
                    avgConsumptionKWh = readings.Any()
                        ? Math.Round(readings.Average(r => r.PowerConsumptionKW), 2)
                        : 0,
                    peakConsumptionKW = readings.Any()
                        ? Math.Round(readings.Max(r => r.PowerConsumptionKW), 2)
                        : 0
                });
            }

            return new ApiResponse(200, "Zone consumption statistics retrieved", new
            {
                startDate = start,
                endDate = end,
                zonesCount = zones.Count,
                zones = zoneConsumption.OrderByDescending(z =>
                    ((dynamic)z).totalConsumptionKWh)
            });
        }
    }
}