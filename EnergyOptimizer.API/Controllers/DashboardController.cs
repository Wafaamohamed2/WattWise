using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Controllers
{
    [EnableRateLimiting("GeneralPolicy")]
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
            try
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
                var avgConsumptionPerHour = hoursElapsed > 0 ? todayConsumption / hoursElapsed : 0;

                var lastReading = await _context.EnergyReadings
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefaultAsync();

                var overview = new DashboardOverviewDto
                {
                    TotalDevices = totalDevices,
                    ActiveDevices = activeDevices,
                    InactiveDevices = totalDevices - activeDevices,
                    TotalZones = totalZones,
                    CurrentTotalConsumption = Math.Round(currentConsumption, 2),
                    TodayTotalConsumption = Math.Round(todayConsumption, 2),
                    AverageConsumptionPerHour = Math.Round(avgConsumptionPerHour, 2),
                    TotalReadingsToday = totalReadingsToday,
                    LastReadingTime = lastReading?.Timestamp ?? DateTime.UtcNow
                };
                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard overview");
                return StatusCode(500, new { error = "Failed to get dashboard overview" });
            }

        }

        /// <summary>
        /// Get consumption by zone
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd (e.g., 2025-01-14)</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd (e.g., 2025-01-15)</param>
        [HttpGet("consumption-by-zone")]
        public async Task<ActionResult<List<ZoneConsumptionDto>>> GetConsumptionByZone(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null)
        {
            try
            {
                DateTime start;
                DateTime end;

                // Parse start date
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out start))
                    {
                        return BadRequest(new { error = "Invalid startDate format. Use yyyy-MM-dd (e.g., 2025-01-14)" });
                    }
                }
                else
                {
                    start = DateTime.UtcNow.Date; // Default to today
                }

                // Parse end date
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateTime.TryParse(endDate, out end))
                    {
                        return BadRequest(new { error = "Invalid endDate format. Use yyyy-MM-dd (e.g., 2025-01-15)" });
                    }
                    end = end.AddDays(1).AddSeconds(-1); // End of day
                }
                else
                {
                    end = DateTime.UtcNow; // Default to now
                }

                // Validate date range
                if (start > end)
                {
                    return BadRequest(new { error = "startDate cannot be after endDate" });
                }

                var zoneConsumption = await _context.Zones
                    .Include(z => z.Devices)
                    .ThenInclude(d => d.EnergyReadings.Where(r => r.Timestamp >= start && r.Timestamp <= end))
                    .Select(z => new
                    {
                        Zone = z,
                        TotalConsumption = z.Devices
                            .SelectMany(d => d.EnergyReadings)
                            .Sum(r => r.PowerConsumptionKW)
                    })
                    .ToListAsync();

                var totalConsumption = zoneConsumption.Sum(z => z.TotalConsumption);

                var result = zoneConsumption.Select(z => new ZoneConsumptionDto
                {
                    ZoneId = z.Zone.Id,
                    ZoneName = z.Zone.Name,
                    ZoneType = z.Zone.Type.ToString(),
                    DeviceCount = z.Zone.Devices.Count,
                    TotalConsumption = Math.Round(z.TotalConsumption, 2),
                    AverageConsumption = z.Zone.Devices.Count > 0
                        ? Math.Round(z.TotalConsumption / z.Zone.Devices.Count, 2)
                        : 0,
                    Percentage = totalConsumption > 0
                        ? Math.Round((z.TotalConsumption / totalConsumption) * 100, 1)
                        : 0
                })
                .OrderByDescending(z => z.TotalConsumption)
                .ToList();

                return Ok(new
                {
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd"),
                    totalZones = result.Count,
                    totalConsumption = Math.Round(totalConsumption, 2),
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consumption by zone");
                return StatusCode(500, new { error = "Failed to get zone consumption" });
            }
        }


        /// <summary>
        /// Get consumption by device
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd</param>
        [HttpGet("consumption-by-device")]
        public async Task<ActionResult<List<DeviceConsumptionDto>>> GetConsumptionByDevice(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null)
        {
            try
            {
                DateTime start;
                DateTime end;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out start))
                    {
                        return BadRequest(new { error = "Invalid startDate format. Use yyyy-MM-dd" });
                    }
                }
                else
                {
                    start = DateTime.UtcNow.Date;
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateTime.TryParse(endDate, out end))
                    {
                        return BadRequest(new { error = "Invalid endDate format. Use yyyy-MM-dd" });
                    }
                    end = end.AddDays(1).AddSeconds(-1);
                }
                else
                {
                    end = DateTime.UtcNow;
                }

                if (start > end)
                {
                    return BadRequest(new { error = "startDate cannot be after endDate" });
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

                var totalConsumption = result.Sum(d => d.TodayConsumption);

                return Ok(new
                {
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd"),
                    totalDevices = result.Count,
                    activeDevices = result.Count(d => d.IsActive),
                    totalConsumption = Math.Round(totalConsumption, 2),
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consumption by device");
                return StatusCode(500, new { error = "Failed to get device consumption" });
            }
        }


        [HttpGet("hourly-consumption")]
        public async Task<ActionResult<List<HourlyConsumptionDto>>> GetHourlyConsumption(
     [FromQuery] string? date = null)
        {
            try
            {
                DateTime targetDate;

                if (!string.IsNullOrEmpty(date))
                {
                    if (!DateTime.TryParse(date, out targetDate))
                    {
                        return BadRequest(new { error = "Invalid date format. Use yyyy-MM-dd (e.g., 2025-10-15)" });
                    }
                }
                else
                {
                    targetDate = DateTime.UtcNow; // ✅ Use today
                }

                targetDate = targetDate.Date;
                var nextDay = targetDate.AddDays(1);

                _logger.LogInformation($"Getting hourly consumption from {targetDate} to {nextDay}");

                var readings = await _context.EnergyReadings
                    .Where(r => r.Timestamp >= targetDate && r.Timestamp < nextDay)
                    .ToListAsync();

                _logger.LogInformation($"Found {readings.Count} readings for date {targetDate:yyyy-MM-dd}");

                var hourlyData = readings
                    .GroupBy(r => r.Timestamp.Hour)
                    .Select(g => new HourlyConsumptionDto
                    {
                        Hour = g.Key,
                        TimeLabel = $"{g.Key:D2}:00",
                        TotalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                        ReadingsCount = g.Count(),
                        AverageConsumption = Math.Round(g.Average(r => r.PowerConsumptionKW), 4)
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                // Fill missing hours with zero
                var completeHours = Enumerable.Range(0, 24)
                    .Select(hour => hourlyData.FirstOrDefault(h => h.Hour == hour) ?? new HourlyConsumptionDto
                    {
                        Hour = hour,
                        TimeLabel = $"{hour:D2}:00",
                        TotalConsumption = 0,
                        ReadingsCount = 0,
                        AverageConsumption = 0
                    })
                    .ToList();

                var totalConsumption = completeHours.Sum(h => h.TotalConsumption);
                var peakHourData = completeHours.Where(h => h.TotalConsumption > 0).OrderByDescending(h => h.TotalConsumption).FirstOrDefault();

                return Ok(new
                {
                    date = targetDate.ToString("yyyy-MM-dd"),
                    totalConsumption = Math.Round(totalConsumption, 2),
                    peakHour = peakHourData?.TimeLabel ?? "N/A",
                    peakConsumption = peakHourData?.TotalConsumption ?? 0,
                    totalReadings = readings.Count,
                    data = completeHours
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hourly consumption");
                return StatusCode(500, new { error = "Failed to get hourly consumption" });
            }
        }


        // Get consumption trend for last 24 hours
        [HttpGet("consumption-trend")]
        public async Task<ActionResult<List<ConsumptionTrendDto>>> GetConsumptionTrend(
            [FromQuery] int hours = 24)
        {
            try
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

                return Ok(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consumption trend");
                return StatusCode(500, new { error = "Failed to get consumption trend" });
            }

        }

        [HttpGet("top-consumers")]
        public async Task<ActionResult<List<DeviceConsumptionDto>>> GetTopConsumers(
      [FromQuery] int count = 5,
      [FromQuery] string? startDate = null)
        {
            try
            {
                DateTime start;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out start))
                    {
                        return BadRequest(new { error = "Invalid startDate format. Use yyyy-MM-dd" });
                    }
                    start = start.Date; // Start of day
                }
                else
                {
                    start = DateTime.UtcNow.Date;
                }

                var end = DateTime.UtcNow; // Until now

                var topDevices = await _context.Devices
                    .Include(d => d.Zone)
                    .Include(d => d.EnergyReadings)
                    .ToListAsync();

                var result = topDevices
                    .Select(d =>
                    {
                        var readings = d.EnergyReadings
                            .Where(r => r.Timestamp >= start && r.Timestamp <= end)
                            .ToList();

                        var totalConsumption = readings.Sum(r => r.PowerConsumptionKW);
                        var latestReading = readings.OrderByDescending(r => r.Timestamp).FirstOrDefault();

                        return new DeviceConsumptionDto
                        {
                            DeviceId = d.Id,
                            DeviceName = d.Name,
                            DeviceType = d.Type.ToString(),
                            ZoneName = d.Zone.Name,
                            RatedPowerKW = d.RatedPowerKW, // ✅ Fixed
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
                    .Where(d => d.TodayConsumption > 0) // ✅ Only show devices with consumption
                    .OrderByDescending(d => d.TodayConsumption)
                    .Take(count)
                    .ToList();

                return Ok(new
                {
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd HH:mm:ss"),
                    count = result.Count,
                    totalConsumption = Math.Round(result.Sum(d => d.TodayConsumption), 2),
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top consumers");
                return StatusCode(500, new { error = "Failed to get top consumers" });
            }
        }


    }
}

    

