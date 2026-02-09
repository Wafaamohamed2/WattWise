using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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


            var result = readings.Select(r => new {
                r.Id,
                r.Timestamp,
                r.PowerConsumptionKW,
                r.Voltage,
                r.Current,
                r.Temperature,
                Device = r.Device != null ? new { r.Device.Id, r.Device.Name, Type = r.Device.Type.ToString() } : null,
                Zone = r.Device?.Zone != null ? new { r.Device.Zone.Id, r.Device.Zone.Name } : null
            });

            return Ok(new ApiResponse(200, $"Latest {readings.Count} readings retrieved", new { count = readings.Count, data = result }));
        }


        // Get readings for a specific device with optional date range and limit
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeviceReadings(
             int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int limit = 100)
        {
            var device = await _deviceRepo.GetEntityWithSpec(new DeviceWithDetailsSpec(deviceId));
            if (device == null) return NotFound(new ApiResponse(404, $"Device with ID {deviceId} not found"));

            if (!TryParseDates(startDate, endDate, out DateTime start, out DateTime end, out string error))
                return BadRequest(new ApiResponse(400, error));

            var spec = new PaginatedReadingsSpec(start, end, deviceId: deviceId, pageSize: limit);
            var readings = await _readingsRepo.ListAsync(spec);

            var stats = new
            {
                TotalReadings = readings.Count,
                TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                AverageConsumption = readings.Any() ? Math.Round(readings.Average(r => r.PowerConsumptionKW), 4) : 0,
                MaxConsumption = readings.Any() ? Math.Round(readings.Max(r => r.PowerConsumptionKW), 4) : 0
            };

            return Ok(new ApiResponse(200, "Device readings retrieved successfully", new
            {
                device = new { device.Id, device.Name, Type = device.Type.ToString(), Zone = device.Zone?.Name },
                startDate = start.ToString("yyyy-MM-dd"),
                endDate = end.ToString("yyyy-MM-dd"),
                statistics = stats,
                data = readings.Select(r => new { r.Id, r.Timestamp, r.PowerConsumptionKW, r.Voltage, r.Current, r.Temperature })
            }));
        }


        
        // Get statistics for a device
        [HttpGet("statistics/{deviceId}")]
        public async Task<ActionResult<object>> GetDeviceStatistics(
            int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] int days = 7)
        {
            var device = await _deviceRepo.GetEntityWithSpec(new DeviceWithDetailsSpec(deviceId));
            if (device == null) return NotFound(new ApiResponse(404, "Device not found"));

            DateTime start = DateTime.TryParse(startDate, out var d) ? d.Date : DateTime.UtcNow.AddDays(-days).Date;
            var end = DateTime.UtcNow;

            var readings = await _readingsRepo.ListAsync(new ReadingsByDeviceAndDateSpec(deviceId, start, end));

            if (!readings.Any())
                return Ok(new ApiResponse(200, "No readings found for this period", new { device = new { device.Id, device.Name } }));

            var dailyStats = readings.GroupBy(r => r.Timestamp.Date).Select(g => new {
                Date = g.Key.ToString("yyyy-MM-dd"),
                TotalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                AverageConsumption = Math.Round(g.Average(r => r.PowerConsumptionKW), 4),
                ReadingsCount = g.Count()
            }).OrderBy(d => d.Date).ToList();

            var overallStats = new
            {
                TotalReadings = readings.Count,
                TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                AverageConsumption = Math.Round(readings.Average(r => r.PowerConsumptionKW), 4),
                AverageVoltage = Math.Round(readings.Average(r => r.Voltage), 2)
            };

            return Ok(new ApiResponse(200, "Device statistics calculated", new
            {
                device = new { device.Id, device.Name, device.RatedPowerKW, Zone = device.Zone?.Name },
                overall = overallStats,
                dailyStats
            }));
        }

       
        // Export readings to CSV "For Future Plan"   
        [HttpGet("export")]
        public async Task<IActionResult> ExportReadings(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? deviceId = null)
        {
            if (!TryParseDates(startDate, endDate, out DateTime start, out DateTime end, out _))
            {
                start = DateTime.UtcNow.AddDays(-7).Date;
                end = DateTime.UtcNow;
            }

            var readings = await _readingsRepo.ListAsync(new PaginatedReadingsSpec(start, end, deviceId: deviceId, pageSize: int.MaxValue));

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Timestamp,Device,DeviceType,Zone,PowerKW,Voltage,Current");
            foreach (var r in readings)
                csv.AppendLine($"{r.Timestamp},{r.Device.Name},{r.Device.Type},{r.Device.Zone?.Name},{r.PowerConsumptionKW},{r.Voltage},{r.Current}");

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"readings_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        // Helper to parse dates
        private bool TryParseDates(string? startStr, string? endStr, out DateTime start, out DateTime end, out string error)
        {
            error = "";
            start = string.IsNullOrEmpty(startStr) ? DateTime.UtcNow.Date : (DateTime.TryParse(startStr, out var s) ? s : DateTime.MinValue);
            end = string.IsNullOrEmpty(endStr) ? DateTime.UtcNow : (DateTime.TryParse(endStr, out var e) ? e.AddDays(1).AddSeconds(-1) : DateTime.MinValue);
            if (start == DateTime.MinValue || end == DateTime.MinValue) { error = "Invalid date format."; return false; }
            return true;
        }
    }
}
