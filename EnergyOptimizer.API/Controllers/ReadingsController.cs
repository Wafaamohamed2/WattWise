using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReadingsController(IMediator mediator, ILogger<ReadingsController> logger)
        {
            _mediator = mediator;
        }


        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<object>>> GetLatestReadings([FromQuery] int limit = 10)
        {
            var result = await _mediator.Send(new GetLatestReadingsQuery(limit));
            return Ok(result);
        }


        // Get readings for a specific device with optional date range and limit
        [HttpGet("device/{deviceId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDeviceReadings(
             int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int limit = 100)
        {
            var result = await _mediator.Send(new GetDeviceReadingsQuery(deviceId, startDate, endDate, limit));
            return Ok(result);
        }


        
        // Get statistics for a device
        [HttpGet("statistics/{deviceId}")]
        public async Task<ActionResult<object>> GetDeviceStatistics(
            int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] int days = 7)
        {

            var start = DateTime.TryParse(startDate, out var d) ? d : DateTime.UtcNow.AddDays(-days);
            var result = await _mediator.Send(new GetDeviceStatisticsQuery(deviceId, start , DateTime.UtcNow));
            return Ok(result);
        }

       
        // Export readings to CSV "For Future Plan"   
        [HttpGet("export")]
        public async Task<IActionResult> ExportReadings(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? deviceId = null)
        {
            var result = await _mediator.Send(new ExportReadingsQuery(deviceId,startDate, endDate));
            var exportData = result.Details as dynamic;

            if (exportData == null) return BadRequest(result);

            return File(exportData.Content, "text/csv", exportData.FileName);
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
