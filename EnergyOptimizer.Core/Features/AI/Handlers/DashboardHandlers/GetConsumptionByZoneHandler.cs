using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetConsumptionByZoneHandler : IRequestHandler<GetConsumptionByZoneQuery, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;

        public GetConsumptionByZoneHandler(IGenericRepository<Zone> zoneRepo, IGenericRepository<EnergyReading> repository)
        {
            _zoneRepo = zoneRepo;
            _readingRepo = repository;
        }

        public async Task<ApiResponse> Handle(GetConsumptionByZoneQuery request, CancellationToken ct)
        {
            if (!DateTime.TryParse(request.StartDate, out var start)) start = DateTime.UtcNow.Date;
            if (!DateTime.TryParse(request.EndDate, out var end)) end = DateTime.UtcNow;

            var zones = await _zoneRepo.ListAsync(new ZonesWithConsumptionSpec());
            var zoneConsumption = new List<ZoneConsumptionItem>();

            var readingsQuery = _readingRepo.GetQueryable()
                .Where(r => r.Timestamp >= start && r.Timestamp <= end);

            foreach (var zone in zones)
            {
                var zoneReadings = readingsQuery.Where(r => r.Device.ZoneId == zone.Id);

                var totalKWh = await zoneReadings.SumAsync(r => (decimal?)r.PowerConsumptionKW, ct) ?? 0m;
                var readingsCount = await zoneReadings.CountAsync(ct);
                var avgKWh = readingsCount > 0 ? await zoneReadings.AverageAsync(r => (decimal?)r.PowerConsumptionKW, ct) ?? 0m : 0m;
                var peakKW = readingsCount > 0 ? await zoneReadings.MaxAsync(r => (decimal?)r.PowerConsumptionKW, ct) ?? 0m : 0m;
                var activeDevices = zone.Devices?.Count(d => d.IsActive) ?? 0;

                zoneConsumption.Add(new ZoneConsumptionItem
                {
                    ZoneId = zone.Id,
                    ZoneName = zone.Name,
                    ZoneType = zone.Type.ToString(),
                    TotalConsumptionKWh = Math.Round(totalKWh, 2),
                    ReadingsCount = readingsCount,
                    ActiveDevices = activeDevices,
                    AvgConsumptionKWh = Math.Round(avgKWh, 2),
                    PeakConsumptionKW = Math.Round(peakKW, 2)
                });
            }

            var orderedZones = zoneConsumption.OrderByDescending(z => z.TotalConsumptionKWh).ToList();

            return new ApiResponse(200, "Zone consumption statistics retrieved", new
            {
                startDate = start,
                endDate = end,
                zonesCount = zones.Count,
                zones = orderedZones
            });
        }

        private class ZoneConsumptionItem
        {
            public int ZoneId { get; set; }
            public string ZoneName { get; set; } = string.Empty;
            public string ZoneType { get; set; } = string.Empty;
            public decimal TotalConsumptionKWh { get; set; }
            public int ReadingsCount { get; set; }
            public int ActiveDevices { get; set; }
            public decimal AvgConsumptionKWh { get; set; }
            public decimal PeakConsumptionKW { get; set; }
        }
    }
}