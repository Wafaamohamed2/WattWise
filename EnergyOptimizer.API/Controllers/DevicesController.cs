using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

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
        [FromQuery] double? minPower = null,
        [FromQuery] double? maxPower = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            try
            {
                var spec = new DeviceFilterSpec(isActive, zoneId, deviceType, minPower, maxPower, page - 1, pageSize);
                var devices = await _deviceRepo.ListAsync(spec);

                var countSpec = new DeviceFilterSpec(isActive, zoneId, deviceType, minPower, maxPower, 0, 0);
                var totalCount = await _deviceRepo.CountAsync(countSpec);

                var activeCountSpec = new DeviceFilterSpec(isActive: true, null, null, null, null, 0, 0);
                var activeCount = await _deviceRepo.CountAsync(activeCountSpec);

                var inactiveCountSpec = new DeviceFilterSpec(isActive: false, null, null, null, null, 0, 0);
                var inactiveCount = await _deviceRepo.CountAsync(inactiveCountSpec);

                return Ok(new
                {
                    count = totalCount,
                    activeCount = activeCount,
                    inactiveCount = inactiveCount,
                    data = devices.Select(d => new
                    {
                        d.Id,
                        DeviceId = d.Id,
                        d.Name,
                        Type = d.Type.ToString(),
                        d.RatedPowerKW,
                        d.IsActive,
                        LastReading = d.EnergyReadings != null ? d.EnergyReadings
                            .OrderByDescending(r => r.Timestamp)
                            .Select(r => new {
                                PowerKW = r.PowerConsumptionKW,
                                r.Voltage,
                                r.Current,
                                r.Temperature,
                                r.Timestamp
                            }).FirstOrDefault() : null,

                        Zone = d.Zone != null ? new
                        {
                            d.Zone.Id,
                            d.Zone.Name,
                            Type = d.Zone.Type.ToString(),
                            Building = d.Zone.Building != null ? new
                            {
                                d.Zone.Building.Id,
                                d.Zone.Building.Name
                            } : null
                        } : null 
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices");
                return StatusCode(500, new { error = "Failed to get devices", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDeviceById(int id)
        {
            try
            {
                var spec = new DeviceWithDetailsSpec(id);
                var device = await _deviceRepo.GetEntityWithSpec(spec);

                if (device == null)
                    return NotFound(new { error = "Device not found" });
                
              return Ok(new
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
              } );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving device with ID {id}");
                return StatusCode(500, new { error = "Failed to get device" });
            }
        }

        [HttpGet("zone/{zoneId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDevicesByZone(int zoneId)
        {
            try
            {
                var zone = await _zoneRepo.GetByIdAsync(zoneId);
                if (zone == null)
                    return NotFound(new { error = $"Zone with ID {zoneId} not found" });

                var spec = new DevicesByZoneSpec(zoneId);
                var devices = await _deviceRepo.ListAsync(spec);

                return Ok(new
                {
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
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving devices for zone ID {zoneId}");
                return StatusCode(500, new { error = "Failed to get devices for the specified zone" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateDevice([FromBody] CreateDeviceDto dto)
        {
            try
            {
                var zone = await _zoneRepo.GetByIdAsync(dto.ZoneId);
                if (zone == null)
                    return BadRequest(new { error = $"Zone with ID {dto.ZoneId} does not exist" });

                var newDevice = new Device
                {
                    Name = dto.Name,
                    ZoneId = dto.ZoneId,
                    Type = dto.Type,
                    RatedPowerKW = dto.RatedPowerKW,
                    IsActive = dto.IsActive,
                    InstallationDate = dto.InstallationDate ?? DateTime.UtcNow
                };

                _deviceRepo.AddAsync(newDevice);
                await _deviceRepo.SaveChangesAsync();

                return CreatedAtAction(nameof(GetDeviceById), new { id = newDevice.Id }, new
                {
                    newDevice.Id,
                    newDevice.Name,
                    Type = newDevice.Type.ToString(),
                    newDevice.RatedPowerKW,
                    newDevice.IsActive,
                    message = "Device created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating new device");
                return StatusCode(500, new { error = "Failed to create device" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            try
            {
                var device = await _deviceRepo.GetByIdAsync(id);
                if (device == null)
                    return NotFound(new { error = "Device not found" });

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

                return Ok(new
                {
                    device.Id,
                    device.Name,
                    Type = device.Type.ToString(),
                    device.RatedPowerKW,
                    device.IsActive,
                    message = "Device updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating device with ID {id}");
                return StatusCode(500, new { error = "Failed to update device" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteDevice(int id)
        {
            try
            {
                var device = await _deviceRepo.GetByIdAsync(id);
                if (device == null)
                    return NotFound(new { error = $"Device with ID {id} not found" });

                device.IsActive = !device.IsActive;
                _deviceRepo.Update(device);
                await _deviceRepo.SaveChangesAsync();

                _logger.LogInformation(
                    "Device {DeviceName} (ID: {DeviceId}) status changed to {Status}",
                    device.Name,
                    device.Id,
                    device.IsActive ? "Active" : "Inactive");

                return Ok(new
                {
                    device.Id,
                    device.Name,
                    isActive = device.IsActive,
                    message = $"Device {(device.IsActive ? "activated" : "deactivated")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting device with ID {id}");
                return StatusCode(500, new { error = "Failed to delete device" });
            }
        }

        //  Toggle device status (Active/Inactive) + notify clients via SignalR
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleDevice(int id)
        {
            try
            {
                var spec = new DeviceWithDetailsSpec(id);
                var device = await _deviceRepo.GetEntityWithSpec(spec);

                if (device == null)
                    return NotFound(new { error = "Device not found" });

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

                return Ok(new
                {
                    id = device.Id,
                    name = device.Name,
                    isActive = device.IsActive,
                    lastReading = lastReading,
                    message = $"Device {(device.IsActive ? "activated" : "deactivated")} successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling device {DeviceId}", id);
                return StatusCode(500, new { error = "Failed to toggle device" });
            }
        }
    }
}