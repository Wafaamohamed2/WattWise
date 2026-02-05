using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.API.Middleware;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly EnergyDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(EnergyDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// Get dashboard overview statistics

        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
        {
                var today = DateTime.UtcNow.Date;

                var totalDevices = await _context.Devices.CountAsync();
                var activeDevices = await _context.Devices.CountAsync(d => d.IsActive);
                var totalZones = await _context.Zones.CountAsync();

                // Get latest readings for current consumption
                var latestReadings = await _context.EnergyReadings
                    .Where(r => r.Timestamp >= DateTime.UtcNow.AddMinutes(-5))
                    .GroupBy(r => r.Device.IsActive)
                    .Select(g => g.OrderByDescending(r => r.Timestamp).FirstOrDefault())
                    .ToListAsync();

                var currentConsumption = latestReadings.Sum(r => r?.PowerConsumptionKW ?? 0);


                // Today's consumption
                var todayReadings = await _context.EnergyReadings
                    .Where(r => r.Timestamp >= today)
                    .ToListAsync();

                var todayConsumption = todayReadings.Sum(r => r.PowerConsumptionKW);
                var totalReadingsToday = todayReadings.Count;

                var hoursElapsed = (DateTime.UtcNow - today).TotalHours;
                var avgConsumptionPerHour = hoursElapsed > 0 ? todayConsumption / (decimal)hoursElapsed : 0;

                var lastReading = await _context.EnergyReadings
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefaultAsync();

                var overview = new DashboardOverviewDto
                {
                    TotalDevices = totalDevices,
                    ActiveDevices = activeDevices,
                    InactiveDevices = totalDevices - activeDevices,
                    TotalZones = totalZones,
                    CurrentTotalConsumption = (double)Math.Round(currentConsumption, 2),
                    TodayTotalConsumption = Math.Round(todayConsumption, 2),
                    AverageConsumptionPerHour = Math.Round(avgConsumptionPerHour, 2),
                    TotalReadingsToday = totalReadingsToday,
                    LastReadingTime = lastReading?.Timestamp ?? DateTime.UtcNow
                };
            return Ok(new ApiResponse(200, "Dashboard overview retrieved", overview));
        }

        /// <summary>
        /// Get consumption by zone
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd (e.g., 2025-01-14)</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd (e.g., 2025-01-15)</param>
        [HttpGet("consumption-by-zone")]
        public async Task<ActionResult> GetConsumptionByZone([FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            if (!TryParseDates(startDate, endDate, out DateTime start, out DateTime end, out string error))
                return BadRequest(new ApiResponse(400, error));

            var zoneData = await _context.Zones
                .Include(z => z.Devices)
                .ThenInclude(d => d.EnergyReadings.Where(r => r.Timestamp >= start && r.Timestamp <= end))
                .ToListAsync();

            var zoneConsumption = zoneData.Select(z => new {
                Zone = z,
                Total = z.Devices.SelectMany(d => d.EnergyReadings).Sum(r => r.PowerConsumptionKW)
            }).ToList();

            var grandTotal = zoneConsumption.Sum(z => z.Total);

            var result = zoneConsumption.Select(z => new ZoneConsumptionDto
            {
                ZoneId = z.Zone.Id,
                ZoneName = z.Zone.Name,
                ZoneType = z.Zone.Type.ToString(),
                DeviceCount = z.Zone.Devices.Count,
                TotalConsumption = Math.Round(z.Total, 2),
                AverageConsumption = z.Zone.Devices.Count > 0 ? Math.Round(z.Total / z.Zone.Devices.Count, 2) : 0,
                Percentage = grandTotal > 0 ? Math.Round((z.Total / grandTotal) * 100, 1) : 0
            }).OrderByDescending(z => z.TotalConsumption).ToList();

            return Ok(new ApiResponse(200, "Zone consumption statistics retrieved", new
            {
                startDate = start.ToString("yyyy-MM-dd"),
                endDate = end.ToString("yyyy-MM-dd"),
                totalConsumption = Math.Round(grandTotal, 2),
                data = result
            }));
        }

        /// <summary>
        /// Get consumption by device
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd</param>
        [HttpGet("consumption-by-device")]
        public async Task<ActionResult> GetConsumptionByDevice(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
        {
            if (!TryParseDates(startDate, endDate, out DateTime start, out DateTime end, out string error))
            {
                return BadRequest(new ApiResponse(400, error));
            }

            var devices = await _context.Devices
                .Include(d => d.Zone)
                .Include(d => d.EnergyReadings.Where(r => r.Timestamp >= start && r.Timestamp <= end))
                .ToListAsync();

            var result = devices.Select(d =>
            {
                var readings = d.EnergyReadings.ToList();
                var totalConsumption = readings.Sum(r => r.PowerConsumptionKW);
                var latestReading = readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();

                return new DeviceConsumptionDto
                {
                    DeviceId = d.Id,
                    DeviceName = d.Name,
                    DeviceType = d.Type.ToString(),
                    ZoneName = d.Zone.Name,
                    RatedPowerKW = d.RatedPowerKW,
                    CurrentConsumption = latestReading?.PowerConsumptionKW ?? 0,
                    TodayConsumption = Math.Round(totalConsumption, 2),
                    AverageConsumption = readings.Count > 0
                        ? Math.Round(totalConsumption / readings.Count, 4)
                        : 0,
                    ReadingsCount = readings.Count,
                    LastReadingTime = latestReading?.Timestamp,
                    IsActive = d.IsActive
                };
            })
            .OrderByDescending(d => d.TodayConsumption)
            .ToList();

            var totalConsumptionAllDevices = result.Sum(d => d.TodayConsumption);

            return Ok(new ApiResponse(200, "Device consumption statistics retrieved successfully", new
            {
                startDate = start.ToString("yyyy-MM-dd"),
                endDate = end.ToString("yyyy-MM-dd"),
                totalDevices = result.Count,
                activeDevices = result.Count(d => d.IsActive),
                totalConsumption = Math.Round(totalConsumptionAllDevices, 2),
                data = result
            }));
        }


        [HttpGet("hourly-consumption")]
        public async Task<ActionResult> GetHourlyConsumption([FromQuery] string? date = null)
        {
            if (!DateTime.TryParse(date, out DateTime targetDate)) targetDate = DateTime.UtcNow;
            targetDate = targetDate.Date;
            var nextDay = targetDate.AddDays(1);

            var readings = await _context.EnergyReadings
                .Where(r => r.Timestamp >= targetDate && r.Timestamp < nextDay)
                .ToListAsync();

            var hourlyData = Enumerable.Range(0, 24).Select(hour => {
                var hourReadings = readings.Where(r => r.Timestamp.Hour == hour).ToList();
                return new HourlyConsumptionDto
                {
                    Hour = hour,
                    TimeLabel = $"{hour:D2}:00",
                    TotalConsumption = Math.Round(hourReadings.Sum(r => r.PowerConsumptionKW), 2),
                    ReadingsCount = hourReadings.Count,
                    AverageConsumption = hourReadings.Count > 0 ? Math.Round(hourReadings.Average(r => r.PowerConsumptionKW), 4) : 0
                };
            }).ToList();

            var total = hourlyData.Sum(h => h.TotalConsumption);
            var peak = hourlyData.OrderByDescending(h => h.TotalConsumption).FirstOrDefault();

            return Ok(new ApiResponse(200, "Hourly consumption retrieved", new
            {
                date = targetDate.ToString("yyyy-MM-dd"),
                totalConsumption = Math.Round(total, 2),
                peakHour = peak?.TimeLabel,
                peakConsumption = peak?.TotalConsumption,
                data = hourlyData
            }));
        }

        // Get consumption trend for last 24 hours
        [HttpGet("consumption-trend")]
        public async Task<ActionResult<List<ConsumptionTrendDto>>> GetConsumptionTrend(
            [FromQuery] int hours = 24)
        {
                var startTime = DateTime.UtcNow.AddHours(-hours);
                var readings = await _context.EnergyReadings
                    .Where(r => r.Timestamp >= startTime)
                    .OrderBy(r => r.Timestamp)
                    .ToListAsync();

                var trend = readings
                    .GroupBy(r => new DateTime(r.Timestamp.Year, r.Timestamp.Month, r.Timestamp.Day, r.Timestamp.Hour, 0, 0))
                    .Select(g => new ConsumptionTrendDto
                    {
                        Timestamp = g.Key,
                        TotalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                        ActiveDevices = g.Select(r => r.DeviceId).Distinct().Count()
                    })
                    .OrderBy(t => t.Timestamp)
                    .ToList();

            return Ok(new ApiResponse(200, $"Consumption trend for last {hours} hours", trend));
        }

        [HttpGet("top-consumers")]
        public async Task<ActionResult<List<DeviceConsumptionDto>>> GetTopConsumers(
        [FromQuery] int count = 5,
        [FromQuery] string? startDate = null)
        {

            DateTime start = DateTime.TryParse(startDate, out var d) ? d.Date : DateTime.UtcNow.Date;
            var end = DateTime.UtcNow;

            var devices = await _context.Devices
                .Include(d => d.Zone)
                .Include(d => d.EnergyReadings.Where(r => r.Timestamp >= start && r.Timestamp <= end))
                .ToListAsync();

            var result = devices.Select(d => {
                var total = d.EnergyReadings.Sum(r => r.PowerConsumptionKW);
                return new DeviceConsumptionDto
                {
                    DeviceId = d.Id,
                    DeviceName = d.Name,
                    ZoneName = d.Zone.Name,
                    TodayConsumption = Math.Round(total, 2),
                    IsActive = d.IsActive
                };
            }).Where(d => d.TodayConsumption > 0)
              .OrderByDescending(d => d.TodayConsumption)
              .Take(count).ToList();

            return Ok(new ApiResponse(200, $"Top {count} consumers retrieved", result));
        }

        private bool TryParseDates(string? startStr, string? endStr, out DateTime start, out DateTime end, out string error)
        {
            error = "";
            start = string.IsNullOrEmpty(startStr) ? DateTime.UtcNow.Date : (DateTime.TryParse(startStr, out var s) ? s : DateTime.MinValue);
            end = string.IsNullOrEmpty(endStr) ? DateTime.UtcNow : (DateTime.TryParse(endStr, out var e) ? e.AddDays(1).AddSeconds(-1) : DateTime.MinValue);

            if (start == DateTime.MinValue || end == DateTime.MinValue) { error = "Invalid date format."; return false; }
            if (start > end) { error = "Start date cannot be after end date."; return false; }
            return true;
        }

    }
}

    

