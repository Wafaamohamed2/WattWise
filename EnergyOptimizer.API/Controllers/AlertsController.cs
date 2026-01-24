using EnergyOptimizer.API.DTOs;
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
    public class AlertsController : ControllerBase
    {
        private readonly EnergyDbContext _context;
        private readonly ILogger<AlertsController> _logger;

        public AlertsController(EnergyDbContext context, ILogger<AlertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Get all alerts with filters
        [HttpGet]
        public async Task<ActionResult<object>> GetAlerts(
            [FromQuery] bool? isRead = null,
            [FromQuery] int? severity = null,
            [FromQuery] int? deviceId = null,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
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

                var query = _context.Alerts
                   .Include(a => a.Device)
                   .ThenInclude(d => d.Zone)
                   .Where(a => a.CreatedAt >= start && a.CreatedAt <= end)
                   .AsQueryable();

                // Apply filters
                if (isRead.HasValue)
                    query = query.Where(a => a.IsRead == isRead.Value);

                if (severity.HasValue)
                    query = query.Where(a => a.Severity == severity.Value);

                if (deviceId.HasValue)
                    query = query.Where(a => a.DeviceId == deviceId.Value);

                var totalAlerts = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalAlerts / (double)pageSize);

                var alerts = await query
                    .OrderByDescending(a => a.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new AlertDto
                    {
                        Id = a.Id,                   
                        DeviceName = a.Device.Name,
                        ZoneName = a.Device.Zone.Name,
                        AlertType = a.Type.ToString(),
                        Message = a.Message,
                        Severity = a.Severity,
                        SeverityLabel = a.Severity == 1 ? "Info" : a.Severity == 2 ? "Warning" : "Critical",
                        Icon = a.Severity == 1 ? "🔵" : a.Severity == 2 ? "🟡" : "🔴",
                        CreatedAt = a.CreatedAt,
                        IsRead = a.IsRead
                    })
                    .ToListAsync();
                    return Ok(new
                    {
                       filters = new
                       {
                            startDate = start.ToString("yyyy-MM-dd"),
                            endDate = end.ToString("yyyy-MM-dd"),
                            isRead,
                            severity,
                            deviceId
                       },
                       pagination = new
                       {
                            currentPage = page,
                            pageSize,
                            totalAlerts,
                            totalPages
                       },
                        data = alerts
                    });
        }

        // Get unread alerts count
        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
                var count = await _context.Alerts
                    .Where(a => !a.IsRead)
                    .CountAsync();

                return Ok(new { unreadCount = count });
        }

        // Get alerts statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<AlertStatistics>> GetStatistics(
           [FromQuery] string? startDate = null,
           [FromQuery] int days = 7)
        {
                DateTime start;
                if (!string.IsNullOrEmpty(startDate))
                {
                    DateTime.TryParse(startDate, out start);
                }
                else
                {
                    start = DateTime.UtcNow.AddDays(-days).Date;
                }

                var alerts = await _context.Alerts
                   .Where(a => a.CreatedAt >= start)
                   .ToListAsync();

                var statistics = new AlertStatistics
                {
                    TotalAlerts = alerts.Count,
                    UnreadAlerts = alerts.Count(a => !a.IsRead),
                    CriticalAlerts = alerts.Count(a => a.Severity == 3),
                    WarningAlerts = alerts.Count(a => a.Severity == 2),
                    InfoAlerts = alerts.Count(a => a.Severity == 1)
                };
                return StatusCode(200, statistics);
        }

        // Get alert by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AlertDto>> GetAlert(int id)
        {
                var alert = await _context.Alerts
                   .Include(a => a.Device)
                   .ThenInclude(d => d.Zone)
                   .Where(a => a.Id == id)
                   .Select(a => new AlertDto
                   {
                       Id = a.Id,
                       DeviceName = a.Device.Name,
                       ZoneName = a.Device.Zone.Name,
                       AlertType = a.Type.ToString(),
                       Message = a.Message,
                       Severity = a.Severity,
                       SeverityLabel = a.Severity == 1 ? "Info" : a.Severity == 2 ? "Warning" : "Critical",
                       Icon = a.Severity == 1 ? "🔵" : a.Severity == 2 ? "🟡" : "🔴",
                       CreatedAt = a.CreatedAt,
                       IsRead = a.IsRead
                   })
                   .FirstOrDefaultAsync();

                if (alert == null)
                    return NotFound(new { error = $"Alert with ID {id} not found" });

                return Ok(alert);
        }

        // Mark alert as read
        [HttpPatch("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
                var alert = await _context.Alerts.FindAsync(id);

                if (alert == null)
                    return NotFound(new { error = $"Alert with ID {id} not found" });

                alert.IsRead = true;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} marked as read", id);

                return Ok(new
                {
                    id = alert.Id,
                    isRead = alert.IsRead,
                    message = "Alert marked as read"
                });
        }

        // Mark all alerts as read
        [HttpPost("all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
                var unreadAlerts = await _context.Alerts
                    .Where(a => !a.IsRead)
                    .ToListAsync();

                foreach (var alert in unreadAlerts)
                {
                    alert.IsRead = true;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Marked {Count} alerts as read", unreadAlerts.Count);

                return Ok(new
                {
                    count = unreadAlerts.Count,
                    message = $"{unreadAlerts.Count} alerts marked as read"
                });
        }

        // Delete alert 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlert(int id)
        {
                var alert = await _context.Alerts.FindAsync(id);

                if (alert == null)
                    return NotFound(new { error = $"Alert with ID {id} not found" });

                _context.Alerts.Remove(alert);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Alert {AlertId} deleted", id);

                return Ok(new
                {
                    id,
                    message = "Alert deleted successfully"
                });
        }

        // Delete all read alerts
        [HttpDelete("clear-read")]
        public async Task<ActionResult> ClearReadAlerts()
        {
                var readAlerts = await _context.Alerts
                    .Where(a => a.IsRead)
                    .ToListAsync();

                _context.Alerts.RemoveRange(readAlerts);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted {Count} read alerts", readAlerts.Count);

                return Ok(new
                {
                    count = readAlerts.Count,
                    message = $"{readAlerts.Count} read alerts deleted"
                });
        }
    }
}
