using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<Zone> _zoneRepo; private readonly ILogger<DevicesController> _logger;
        private readonly IHubContext<EnergyHub> _hubContext;

        public DevicesController(IGenericRepository<Device> deviceRepo,
            IGenericRepository<Zone> zoneRepo, ILogger<DevicesController> logger, IHubContext<EnergyHub> hubContext)
        {
            _deviceRepo = deviceRepo;
            _zoneRepo = zoneRepo;
            _logger = logger;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllDevices(
        [FromQuery] bool? isActive = null,
        [FromQuery] int? zoneId = null,
        [FromQuery] DeviceType? deviceType = null,
        [FromQuery] decimal? minPower = null,
        [FromQuery] decimal? maxPower = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
                var spec = new DeviceFilterSpec(isActive, zoneId, deviceType, minPower, maxPower, page - 1, pageSize);
                var devices = await _deviceRepo.ListAsync(spec);

                var totalCount = await _deviceRepo.CountAsync(new DeviceFilterSpec(isActive, zoneId, deviceType, minPower, maxPower, 0, 0));
                var activeCount = await _deviceRepo.CountAsync(new DeviceFilterSpec(true, null, null, null, null, 0, 0));
                var inactiveCount = await _deviceRepo.CountAsync(new DeviceFilterSpec(false, null, null, null, null, 0, 0));
            return Ok(new ApiResponse(200, "Devices retrieved successfully", new
            {
                count = totalCount,
                activeCount,
                inactiveCount,
                data = devices.Select(d => new {
                    d.Id,
                    d.Name,
                    Type = d.Type.ToString(),
                    d.RatedPowerKW,
                    d.IsActive,
                    Zone = d.Zone?.Name
                })
            }));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDeviceById(int id)
        {
            
                var spec = new DeviceWithDetailsSpec(id);
                var device = await _deviceRepo.GetEntityWithSpec(spec);

                if (device == null)
                    return NotFound(new ApiResponse(404, "Device not found"));
            return Ok(new ApiResponse(200, "Device details retrieved successfully", new
            {
                device.Id,
                device.Name,
                Type = device.Type.ToString(),
                device.IsActive,
                Zone = new
                {
                    device.Zone.Id,
                    device.Zone.Name,
                    Type = device.Zone.Type.ToString(),
                    device.Zone.Area,
                    Building = new
                    {
                        device.Zone.Building.Id,
                        device.Zone.Building.Name
                    },
                },
                Statistics = new
                {
                    TotalReadings = device.EnergyReadings.Count,
                    LastReading = device.EnergyReadings
               .OrderByDescending(r => r.Timestamp)
               .Select(r => new
               {
                   r.Timestamp,
                   r.PowerConsumptionKW
               })
               .FirstOrDefault(),
                    TodayConsumption = device.EnergyReadings
               .Where(r => r.Timestamp >= DateTime.UtcNow.Date)
               .Sum(r => r.PowerConsumptionKW)
                }
            }));
        }

        [HttpGet("zone/{zoneId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDevicesByZone(int zoneId)
        {
            
                var zone = await _zoneRepo.GetByIdAsync(zoneId);
                if (zone == null)
                    return NotFound(new ApiResponse ( 404,  $"Zone with ID {zoneId} not found" ));

                var spec = new DevicesByZoneSpec(zoneId);
                var devices = await _deviceRepo.ListAsync(spec);

                return Ok(new ApiResponse(200, "Devices retrieved successfully",
                    new {

                       zoneId,
                       zoneName = zone.Name,
                       deviceCount = devices.Count,
                       data = devices.Select(d => new
                       {
                           d.Id,
                           d.Name,
                           Type = d.Type.ToString(),
                           d.RatedPowerKW,
                           d.IsActive,
                           d.InstallationDate
                       })
                       
                }));
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateDevice([FromBody] CreateDeviceDto dto)
        {
                var zone = await _zoneRepo.GetByIdAsync(dto.ZoneId);
                if (zone == null)
                    return BadRequest(new ApiResponse(400, $"Zone with ID {dto.ZoneId} does not exist"));
            var newDevice = new Device
                {
                    Name = dto.Name,
                    ZoneId = dto.ZoneId,
                    Type = dto.Type,
                    RatedPowerKW = dto.RatedPowerKW,
                    IsActive = dto.IsActive,
                    InstallationDate = dto.InstallationDate ?? DateTime.UtcNow
                };

                await _deviceRepo.AddAsync(newDevice);
                await _deviceRepo.SaveChangesAsync();

            return CreatedAtAction(nameof(GetDeviceById), new { id = newDevice.Id },
                new ApiResponse(201, "Device created successfully"));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
                var device = await _deviceRepo.GetByIdAsync(id);
                if (device == null)
                    return NotFound(new ApiResponse(404, "Device not found"));

            if (dto.ZoneId.HasValue)
                {
                    var zone = await _zoneRepo.GetByIdAsync(dto.ZoneId.Value);
                    if (zone == null)
                        return BadRequest(new { error = $"Zone with ID {dto.ZoneId.Value} does not exist" });
                    device.ZoneId = dto.ZoneId.Value;
                }

                device.Name = dto.Name ?? device.Name;
                device.Type = dto.Type ?? device.Type;
                device.RatedPowerKW = dto.RatedPowerKW ?? device.RatedPowerKW;
                device.IsActive = dto.IsActive ?? device.IsActive;

                _deviceRepo.Update(device);
                await _deviceRepo.SaveChangesAsync();

            return Ok(new ApiResponse(200, "Device updated successfully"));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteDevice(int id)
        {
                var device = await _deviceRepo.GetByIdAsync(id);
                if (device == null)
                      return NotFound(new ApiResponse(404, "Device not found"));
            device.IsActive = !device.IsActive;
                _deviceRepo.Update(device);
                await _deviceRepo.SaveChangesAsync();

                _logger.LogInformation(
                    "Device {DeviceName} (ID: {DeviceId}) status changed to {Status}",
                    device.Name,
                    device.Id,
                    device.IsActive ? "Active" : "Inactive");

            return Ok(new ApiResponse(200, "Device deleted successfully"));
        }

        //  Toggle device status (Active/Inactive) + notify clients via SignalR
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleDevice(int id)
        {
                var spec = new DeviceWithDetailsSpec(id);
                var device = await _deviceRepo.GetEntityWithSpec(spec);

                if (device == null)
                    return NotFound(new ApiResponse(404, "Device not found"));

            device.IsActive = !device.IsActive;
                _deviceRepo.Update(device);
                await _deviceRepo.SaveChangesAsync();

                var lastReading = device.EnergyReadings
                    .OrderByDescending(r => r.Timestamp)
                    .Select(r => new { PowerKW = r.PowerConsumptionKW })
                    .FirstOrDefault();

                await _hubContext.Clients.All.SendAsync("DeviceStatusUpdated", new
                {
                    DeviceId = device.Id,
                    IsActive = device.IsActive,
                    LastReading = lastReading
                });

            return Ok(new ApiResponse(200, $"Device {(device.IsActive ? "activated" : "deactivated")} successfully"));
        }
    }
}