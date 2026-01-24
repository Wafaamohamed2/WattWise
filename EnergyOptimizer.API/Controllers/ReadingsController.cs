using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingsController : ControllerBase
    {
        private readonly IGenericRepository<EnergyReading> _readingsRepo;
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ILogger<ReadingsController> _logger;

        public ReadingsController(IGenericRepository<EnergyReading> readingsRepo,
            IGenericRepository<Device> deviceRepo, ILogger<ReadingsController> logger)
        {
            _readingsRepo = readingsRepo;
            _deviceRepo = deviceRepo;
            _logger = logger;
        }


        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<object>>> GetLatestReadings([FromQuery] int limit = 50)
        {
                var spec = new LatestReadingsSpec(limit);
                var readings = await _readingsRepo.ListAsync(spec);


                return Ok(new
                {
                    count = readings.Count,
                    data = readings.Select(r => new
                    {
                        r.Id,
                        r.Timestamp,
                        r.PowerConsumptionKW,
                        r.Voltage,
                        r.Current,
                        r.Temperature,
                        Device = new
                        {
                            r.Device.Id,
                            r.Device.Name,
                            Type = r.Device.Type.ToString()
                        },
                        Zone = new
                        {
                            r.Device.Zone.Id,
                            r.Device.Zone.Name
                        }
                    })
                });
        }


        // Get readings for a specific device with optional date range and limit
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeviceReadings(
             int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int limit = 100)
        {
                // Check if device exists
                var deviceSpec = new DeviceWithDetailsSpec(deviceId);
                var device = await _deviceRepo.GetEntityWithSpec(deviceSpec);

                if (device == null)
                    return NotFound(new { error = $"Device with ID {deviceId} not found" });

                // Parse dates
                DateTime start = DateTime.UtcNow.Date;
                DateTime end = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out start))
                        return BadRequest(new { error = "Invalid startDate format" });
                }
                if (!string.IsNullOrEmpty(endDate))
                {
                    if (!DateTime.TryParse(endDate, out end))
                        return BadRequest(new { error = "Invalid endDate format" });
                    end = end.AddDays(1).AddSeconds(-1);
                }

                var spec = new PaginatedReadingsSpec(start, end, deviceId: deviceId, pageSize: limit);
                var readings = await _readingsRepo.ListAsync(spec);

                var stats = new
                {
                    TotalReadings = readings.Count,
                    TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                    AverageConsumption = readings.Any() ? Math.Round(readings.Average(r => r.PowerConsumptionKW), 4) : 0,
                    MaxConsumption = readings.Any() ? Math.Round(readings.Max(r => r.PowerConsumptionKW), 4) : 0,
                    MinConsumption = readings.Any() ? Math.Round(readings.Min(r => r.PowerConsumptionKW), 4) : 0
                };
                return Ok(new
                {
                    device = new
                    {
                        device.Id,
                        device.Name,
                        Type = device.Type.ToString(),
                        Zone = device.Zone.Name
                    },
                    startDate = start.ToString("yyyy-MM-dd"),
                    endDate = end.ToString("yyyy-MM-dd"),
                    statistics = stats,
                    data = readings.Select(r => new
                    {
                        r.Id,
                        r.Timestamp,
                        r.PowerConsumptionKW,
                        r.Voltage,
                        r.Current,
                        r.Temperature
                    })
                });
        }


        
        // Get statistics for a device
        [HttpGet("statistics/{deviceId}")]
        public async Task<ActionResult<object>> GetDeviceStatistics(
            int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] int days = 7)
        {
                var deviceSpec = new DeviceWithDetailsSpec(deviceId);
                var device = await _deviceRepo.GetEntityWithSpec(deviceSpec);

                if (device == null)
                    return NotFound(new { error = $"Device with ID {deviceId} not found" });

                DateTime start;
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (!DateTime.TryParse(startDate, out start))
                        return BadRequest(new { error = "Invalid startDate format" });
                }
                else
                {
                    start = DateTime.UtcNow.AddDays(-days).Date;
                }

                var end = DateTime.UtcNow;

                var readingsSpec = new ReadingsByDeviceAndDateSpec(deviceId, start, end);
                var readings = await _readingsRepo.ListAsync(readingsSpec);

                if (!readings.Any())
                {
                    return Ok(new
                    {
                        device = new { device.Id, device.Name },
                        message = "No readings found for this period",
                        statistics = new { }
                    });
                }

                var dailyStats = readings
                    .GroupBy(r => r.Timestamp.Date)
                    .Select(g => new
                    {
                        Date = g.Key.ToString("yyyy-MM-dd"),
                        TotalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                        AverageConsumption = Math.Round(g.Average(r => r.PowerConsumptionKW), 4),
                        MaxConsumption = Math.Round(g.Max(r => r.PowerConsumptionKW), 4),
                        ReadingsCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var hourlyPattern = readings
                  .GroupBy(r => r.Timestamp.Hour)
                  .Select(g => new
                  {
                      Hour = g.Key,
                      TimeLabel = $"{g.Key:D2}:00",
                      AverageConsumption = Math.Round(g.Average(r => r.PowerConsumptionKW), 4),
                      ReadingsCount = g.Count()
                  })
                  .OrderBy(h => h.Hour)
                  .ToList();

                var overallStats = new
                {
                    TotalReadings = readings.Count,
                    TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                    AverageConsumption = Math.Round(readings.Average(r => r.PowerConsumptionKW), 4),
                    MaxConsumption = Math.Round(readings.Max(r => r.PowerConsumptionKW), 4),
                    MinConsumption = Math.Round(readings.Min(r => r.PowerConsumptionKW), 4),
                    AverageVoltage = Math.Round(readings.Average(r => r.Voltage), 2),
                    AverageTemperature = Math.Round(readings.Average(r => r.Temperature), 2)
                };


                return Ok(new
                {
                    device = new
                    {
                        device.Id,
                        device.Name,
                        Type = device.Type.ToString(),
                        device.RatedPowerKW,
                        Zone = device.Zone.Name
                    },
                    period = new
                    {
                        startDate = start.ToString("yyyy-MM-dd"),
                        endDate = end.ToString("yyyy-MM-dd"),
                        days
                    },
                    overall = overallStats,
                    dailyStats,
                    hourlyPattern
                });
        }

       
        // Export readings to CSV "For Future Plan"   
        [HttpGet("export")]
        public async Task<IActionResult> ExportReadings(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? deviceId = null)
        {
                DateTime start = DateTime.UtcNow.AddDays(-7).Date;
                DateTime end = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(startDate))
                    DateTime.TryParse(startDate, out start);

                if (!string.IsNullOrEmpty(endDate))
                {
                    DateTime.TryParse(endDate, out end);
                    end = end.AddDays(1).AddSeconds(-1);
                }

                var spec = new PaginatedReadingsSpec(start, end, deviceId: deviceId, pageSize: int.MaxValue); 
                var readings = await _readingsRepo.ListAsync(spec);

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Timestamp,Device,DeviceType,Zone,PowerKW,Voltage,Current,Temperature");

                foreach (var r in readings)
                {
                    csv.AppendLine($"{r.Timestamp},{r.Device.Name},{r.Device.Type},{r.Device.Zone.Name},{r.PowerConsumptionKW},{r.Voltage},{r.Current},{r.Temperature}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"readings_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
    }
}
