using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingsController : ControllerBase
    {
        private readonly EnergyDbContext _context;
        private readonly ILogger<ReadingsController> _logger;

        public ReadingsController(EnergyDbContext context, ILogger<ReadingsController> logger)
        {
            _context = context;
            _logger = logger;
        }


        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<object>>> GetLatestReadings([FromQuery] int limit = 50)
        {
            try
            {
                var readings = await _context.EnergyReadings
                   .Include(r => r.Device)
                   .ThenInclude(d => d.Zone)
                   .OrderByDescending(r => r.Timestamp)
                   .Take(limit)
                   .Select(r => new
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
                   }).ToListAsync();


                return Ok(new
                {
                    count = readings.Count,
                    data = readings
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving latest readings");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
        }


        //// Get readings for a specific device with optional date range and limit
        //[HttpGet("device/{deviceId}")]
        //public async Task<ActionResult<IEnumerable<object>>> GetDeviceReadings(
        //     int deviceId,
        //    [FromQuery] string? startDate = null,
        //    [FromQuery] string? endDate = null,
        //    [FromQuery] int limit = 100)
        //{
        //    try
        //    {
        //        // Check if device exists
        //        var device = await _context.Devices
        //            .Include(d => d.Zone)
        //            .FirstOrDefaultAsync(d => d.Id == deviceId);

        //        if (device == null)
        //            return NotFound(new { error = $"Device with ID {deviceId} not found" });

        //        // Parse dates
        //        DateTime start = DateTime.UtcNow.Date;
        //        DateTime end = DateTime.UtcNow;

        //        if (!string.IsNullOrEmpty(startDate))
        //        {
        //            if (!DateTime.TryParse(startDate, out start))
        //                return BadRequest(new { error = "Invalid startDate format" });
        //        }
        //        if (!string.IsNullOrEmpty(endDate))
        //        {
        //            if (!DateTime.TryParse(endDate, out end))
        //                return BadRequest(new { error = "Invalid endDate format" });
        //            end = end.AddDays(1).AddSeconds(-1);
        //        }

        //        var readings = await _context.EnergyReadings
        //                           .Where(r => r.DeviceId == deviceId && r.Timestamp >= start && r.Timestamp <= end)
        //                           .OrderByDescending(r => r.Timestamp)
        //                           .Take(limit)
        //                           .Select(r => new
        //                           {
        //                               r.Id,
        //                               r.Timestamp,
        //                               r.PowerConsumptionKW,
        //                               r.Voltage,
        //                               r.Current,
        //                               r.Temperature
        //                           })
        //                           .ToListAsync();

        //        var stats = new
        //        {
        //            TotalReadings = readings.Count,
        //            TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
        //            AverageConsumption = readings.Any()
        //                ? Math.Round(readings.Average(r => r.PowerConsumptionKW), 4)
        //                : 0,
        //            MaxConsumption = readings.Any()
        //                ? Math.Round(readings.Max(r => r.PowerConsumptionKW), 4)
        //                : 0,
        //            MinConsumption = readings.Any()
        //                ? Math.Round(readings.Min(r => r.PowerConsumptionKW), 4)
        //                : 0
        //        };
        //        return Ok(new
        //        {
        //            device = new
        //            {
        //                device.Id,
        //                device.Name,
        //                Type = device.Type.ToString(),
        //                Zone = device.Zone.Name
        //            },
        //            startDate = start.ToString("yyyy-MM-dd"),
        //            endDate = end.ToString("yyyy-MM-dd"),
        //            statistics = stats,
        //            data = readings
        //        });

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error retrieving readings for device {deviceId}");
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get device readings");
        //    }
        //}


        
        // Get statistics for a device


        [HttpGet("statistics/{deviceId}")]
        public async Task<ActionResult<object>> GetDeviceStatistics(
            int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] int days = 7)
        {
            try
            {
                var device = await _context.Devices
                    .Include(d => d.Zone)
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

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

                var readings = await _context.EnergyReadings
                    .Where(r => r.DeviceId == deviceId && r.Timestamp >= start && r.Timestamp <= end)
                    .ToListAsync();

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics for device {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to get device statistics" });
            }
        }


       
       //// Search readings with advanced filters
       //[HttpGet("search")]
       // public async Task<ActionResult<object>> SearchReadings(
       //     [FromQuery] string? startDate = null,
       //     [FromQuery] string? endDate = null,
       //     [FromQuery] int? deviceId = null,
       //     [FromQuery] int? zoneId = null,
       //     [FromQuery] double? minPower = null,
       //     [FromQuery] double? maxPower = null,
       //     [FromQuery] int page = 1,
       //     [FromQuery] int pageSize = 50)
       // {
       //     try
       //     {
       //         DateTime start = DateTime.UtcNow.AddDays(-1).Date;
       //         DateTime end = DateTime.UtcNow;

       //         if (!string.IsNullOrEmpty(startDate))
       //             DateTime.TryParse(startDate, out start);

       //         if (!string.IsNullOrEmpty(endDate))
       //         {
       //             DateTime.TryParse(endDate, out end);
       //             end = end.AddDays(1).AddSeconds(-1);
       //         }
       //         var query = _context.EnergyReadings
       //             .Include(r => r.Device)
       //             .ThenInclude(d => d.Zone)
       //             .Where(r => r.Timestamp >= start && r.Timestamp <= end)
       //             .AsQueryable();

       //         if (deviceId.HasValue)
       //             query = query.Where(r => r.DeviceId == deviceId.Value);

       //         if (zoneId.HasValue)
       //             query = query.Where(r => r.Device.ZoneId == zoneId.Value);

       //         if (minPower.HasValue)
       //             query = query.Where(r => r.PowerConsumptionKW >= minPower.Value);

       //         if (maxPower.HasValue)
       //             query = query.Where(r => r.PowerConsumptionKW <= maxPower.Value);

       //         var totalCount = await query.CountAsync();
       //         var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

       //         var readings = await query
       //            .OrderByDescending(r => r.Timestamp)
       //            .Skip((page - 1) * pageSize)
       //            .Take(pageSize)
       //            .Select(r => new
       //            {
       //                r.Id,
       //                r.Timestamp,
       //                r.PowerConsumptionKW,
       //                r.Voltage,
       //                r.Current,
       //                r.Temperature,
       //                Device = r.Device.Name,
       //                DeviceType = r.Device.Type.ToString(),
       //                Zone = r.Device.Zone.Name
       //            })
       //           .ToListAsync();

       //         return Ok(new
       //         {
       //             filters = new
       //             {
       //                 startDate = start.ToString("yyyy-MM-dd"),
       //                 endDate = end.ToString("yyyy-MM-dd"),
       //                 deviceId,
       //                 zoneId,
       //                 minPower,
       //                 maxPower
       //             },
       //             pagination = new
       //             {
       //                 currentPage = page,
       //                 pageSize,
       //                 totalCount,
       //                 totalPages
       //             },
       //             data = readings
       //         });
       //     }
       //     catch (Exception ex)
       //     {
       //         _logger.LogError(ex, "Error searching readings");
       //         return StatusCode(500, new { error = "Failed to search readings" });
       //     }
       // }

       
        // Export readings to CSV "For Future Plan"   
        [HttpGet("export")]
        public async Task<IActionResult> ExportReadings(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? deviceId = null)
        {
            try
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

                var query = _context.EnergyReadings
                    .Include(r => r.Device)
                    .ThenInclude(d => d.Zone)
                    .Where(r => r.Timestamp >= start && r.Timestamp <= end)
                    .AsQueryable();

                if (deviceId.HasValue)
                    query = query.Where(r => r.DeviceId == deviceId.Value);

                var readings = await query
                     .OrderBy(r => r.Timestamp)
                     .Select(r => new
                     {
                         Timestamp = r.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                         Device = r.Device.Name,
                         DeviceType = r.Device.Type.ToString(),
                         Zone = r.Device.Zone.Name,
                         PowerKW = r.PowerConsumptionKW,
                         Voltage = r.Voltage,
                         Current = r.Current,
                         Temperature = r.Temperature
                     })
                    .ToListAsync();

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Timestamp,Device,DeviceType,Zone,PowerKW,Voltage,Current,Temperature");

                foreach (var r in readings)
                {
                    csv.AppendLine($"{r.Timestamp},{r.Device},{r.DeviceType},{r.Zone},{r.PowerKW},{r.Voltage},{r.Current},{r.Temperature}");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"readings_{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting readings");
                return StatusCode(500, new { error = "Failed to export readings" });
            }
        }
    }
}
