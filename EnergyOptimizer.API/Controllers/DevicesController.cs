using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly EnergyDbContext _context;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(EnergyDbContext context, ILogger<DevicesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllDevices(
            [FromQuery] bool? isActive = null,
            [FromQuery] int? zoneId = null,
            [FromQuery] DeviceType? deviceType = null)
        {
            try
            {
                var query = _context.Devices
                   .Include(d => d.Zone)
                   .ThenInclude(z => z.Building)
                   .AsQueryable();

                // Apply filters
                if (isActive.HasValue)
                    query = query.Where(d => d.IsActive == isActive.Value);

                if (zoneId.HasValue)
                    query = query.Where(d => d.ZoneId == zoneId.Value);

                if (deviceType.HasValue)
                    query = query.Where(d => d.Type == deviceType.Value);

                var devices = await query.Select(d => new
                {
                    d.Id,
                    d.Name,
                    Type = d.Type.ToString(),
                    d.IsActive,
                    Zone = new
                    {
                        d.Zone.Id,
                        d.Zone.Name,
                        Type = d.Zone.Type.ToString(),
                        Building = new
                        {
                            d.Zone.Building.Id,
                            d.Zone.Building.Name
                        }
                    }
                }).OrderBy(d => d.Zone.Name)
                   .ThenBy(d => d.Name)
                    .ToListAsync();

                return Ok(new
                {
                    count = devices.Count,
                    activeCount = devices.Count(d => d.IsActive),
                    inactiveCount = devices.Count(d => !d.IsActive),
                    data = devices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving devices");
                return StatusCode(500, new { error = "Failed to get devices" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDeviceById(int id)
        {
            try
            {
                var device = await _context.Devices
                    .Include(d => d.Zone)
                    .ThenInclude(z => z.Building)
                    .Where(d => d.Id == id)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        Type = d.Type.ToString(),
                        d.IsActive,
                        Zone = new
                        {
                            d.Zone.Id,
                            d.Zone.Name,
                            Type = d.Zone.Type.ToString(),
                            d.Zone.Area,
                            Building = new
                            {
                                d.Zone.Building.Id,
                                d.Zone.Building.Name
                            },
                            
                        },
                        Statistics = new
                        {
                            TotalReadings = d.EnergyReadings.Count,
                            LastReading = d.EnergyReadings
                                .OrderByDescending(r => r.Timestamp)
                                .Select(r => new
                                {
                                    r.Timestamp,
                                    r.PowerConsumptionKW
                                })
                                .FirstOrDefault(),
                            TodayConsumption = d.EnergyReadings
                                .Where(r => r.Timestamp >= DateTime.UtcNow.Date)
                                .Sum(r => r.PowerConsumptionKW)
                        }
                    })
                    .FirstOrDefaultAsync();

                if (device == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                return Ok(device);
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
                var zone = await _context.Zones.FindAsync(zoneId);
                if (zone == null)
                    return NotFound(new { error = $"Zone with ID {zoneId} not found" });

                var devices = await _context.Devices
                    .Where(d => d.ZoneId == zoneId)
                    .Select(d => new
                    {
                        d.Id,
                        d.Name,
                        Type = d.Type.ToString(),
                        d.RatedPowerKW,
                        d.IsActive,
                        d.InstallationDate
                    })
                    .ToListAsync();


                return Ok(new
                {
                    zoneId,
                    zoneName = zone.Name,
                    deviceCount = devices.Count,
                    data = devices

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
                var zone = await _context.Zones.FindAsync(dto.ZoneId);
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

                _context.Devices.Add(newDevice);
                await _context.SaveChangesAsync();

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
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                    return NotFound(new { error = "Device not found" });

                if (dto.ZoneId.HasValue)
                {
                    var zone = await _context.Zones.FindAsync(dto.ZoneId.Value);
                    if (zone == null)
                        return BadRequest(new { error = $"Zone with ID {dto.ZoneId.Value} does not exist" });
                    device.ZoneId = dto.ZoneId.Value;
                }

                device.Name = dto.Name ?? device.Name;
                device.Type = dto.Type ?? device.Type;
                device.RatedPowerKW = dto.RatedPowerKW ?? device.RatedPowerKW;
                device.IsActive = dto.IsActive ?? device.IsActive;
              
                await _context.SaveChangesAsync();

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
                var device = await _context.Devices.FindAsync(id);
                if (device == null)
                    return NotFound(new { error = $"Device with ID {id} not found" });

                device.IsActive = !device.IsActive;
                await _context.SaveChangesAsync();

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

        // Toggle device status (Active/Inactive)
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleDevice(int id)
        {
            try
            {
                var device = await _context.Devices
                    .Include(d => d.Zone)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (device == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                device.IsActive = !device.IsActive;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Device {DeviceName} (ID: {DeviceId}) status toggled to {Status}",
                    device.Name,
                    device.Id,
                    device.IsActive ? "Active" : "Inactive");

                return Ok(new
                    {                
                    device.Name,
                    isActive = device.IsActive,
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
