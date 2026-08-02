using MediatR;
using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetDeviceStatisticsHandler : IRequestHandler<GetDeviceStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDeviceStatisticsHandler(
            IGenericRepository<EnergyReading> readingRepo, 
            IGenericRepository<Device> deviceRepo,
            ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetDeviceStatisticsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var device = await _deviceRepo.GetEntityWithSpec(new DeviceWithDetailsSpec(request.DeviceId, userId));

            if (device == null)
                throw new NotFoundException($"Device with ID {request.DeviceId} not found");

            DateTime start = request.StartDate ?? DateTime.UtcNow.AddDays(-request.Days);
            DateTime end = DateTime.UtcNow;

            var query = _readingRepo.GetQueryable()
                .Where(r => r.DeviceId == request.DeviceId &&
                            r.Timestamp >= start && r.Timestamp <= end &&
                            r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null &&
                            r.Device.Zone.Building.UserId == userId);

            var dailyStatsDb = await query
                .GroupBy(r => r.Timestamp.Date)
                .Select(g => new {
                    Date = g.Key,
                    TotalConsumption = g.Sum(r => r.PowerConsumptionKW),
                    AverageConsumption = g.Average(r => r.PowerConsumptionKW)
                })
                .OrderBy(d => d.Date)
                .ToListAsync(ct);

            if (!dailyStatsDb.Any())
                return new ApiResponse(200, "No readings found", new { device = new { device.Id, device.Name } });

            var overallStats = await query
                .GroupBy(r => 1)
                .Select(g => new {
                    TotalReadings = g.Count(),
                    TotalConsumption = g.Sum(r => r.PowerConsumptionKW),
                    AverageVoltage = g.Average(r => r.Voltage)
                }).FirstOrDefaultAsync(ct);

            var dailyStats = dailyStatsDb.Select(d => new {
                Date = d.Date.ToString("yyyy-MM-dd"),
                TotalConsumption = Math.Round(d.TotalConsumption, 2),
                AverageConsumption = Math.Round(d.AverageConsumption, 4)
            }).ToList();

            var result = new
            {
                device = new { device.Id, device.Name, device.RatedPowerKW, Zone = device.Zone?.Name },
                overall = new
                {
                    TotalReadings = overallStats?.TotalReadings ?? 0,
                    TotalConsumption = Math.Round(overallStats?.TotalConsumption ?? 0m, 2),
                    AverageVoltage = Math.Round(overallStats?.AverageVoltage ?? 0m, 2)
                },
                dailyStats
            };

            return new ApiResponse(200, "Statistics calculated", result);
        }
    }
}