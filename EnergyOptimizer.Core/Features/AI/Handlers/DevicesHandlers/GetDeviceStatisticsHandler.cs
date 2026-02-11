using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using MediatR;


namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetDeviceStatisticsHandler : IRequestHandler<GetDeviceStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Device> _deviceRepo;

        public GetDeviceStatisticsHandler(IGenericRepository<EnergyReading> readingRepo, IGenericRepository<Device> deviceRepo)
        {
            _readingRepo = readingRepo;
            _deviceRepo = deviceRepo;
        }

        public async Task<ApiResponse> Handle(GetDeviceStatisticsQuery request, CancellationToken ct)
        {
            var device = await _deviceRepo.GetEntityWithSpec(new DeviceWithDetailsSpec(request.DeviceId));
            if (device == null) return new ApiResponse(404, "Device not found");

            var readings = await _readingRepo.ListAsync(new ReadingsByDeviceAndDateSpec(request.DeviceId, request.StartDate, request.EndDate));

            if (!readings.Any())
                return new ApiResponse(200, "No readings found", new { device = new { device.Id, device.Name } });

            var dailyStats = readings.GroupBy(r => r.Timestamp.Date).Select(g => new {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                AverageConsumption = Math.Round(g.Average(r => r.PowerConsumptionKW), 4)
            }).OrderBy(d => d.Date).ToList();

            var result = new
            {
                device = new { device.Id, device.Name, device.RatedPowerKW, Zone = device.Zone?.Name },
                overall = new
                {
                    TotalReadings = readings.Count,
                    TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                    AverageVoltage = Math.Round(readings.Average(r => r.Voltage), 2)
                },
                dailyStats
            };

            return new ApiResponse(200, "Statistics calculated", result);
        }
    }
}
