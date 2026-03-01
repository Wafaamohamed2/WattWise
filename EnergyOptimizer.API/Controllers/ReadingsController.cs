using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.Core.Exceptions;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReadingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReadingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestReadings(
           [FromQuery] int limit = 10,
           [FromQuery] string? startDate = null,
           [FromQuery] string? endDate = null)
        {
            var result = await _mediator.Send(new GetLatestReadingsQuery(limit, startDate, endDate));
            return Ok(result);
        }

        [HttpGet("device/{deviceId}")]
        public async Task<IActionResult> GetDeviceReadings(
             int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int limit = 100)
        {
            var result = await _mediator.Send(new GetDeviceReadingsQuery(deviceId, startDate, endDate, limit));
            return Ok(result);
        }

        [HttpGet("statistics/{deviceId}")]
        public async Task<IActionResult> GetDeviceStatistics(
            int deviceId,
            [FromQuery] string? startDate = null,
            [FromQuery] int days = 7)
        {
            DateTime? start = null;
            if (DateTime.TryParse(startDate, out var d)) start = d;

            var result = await _mediator.Send(new GetDeviceStatisticsQuery(deviceId, start, days));
            return Ok(result);
        }

        [HttpGet("export")]
        public async Task<IActionResult> ExportReadings(
            [FromQuery] string? startDate = null,
            [FromQuery] string? endDate = null,
            [FromQuery] int? deviceId = null)
        {
            var result = await _mediator.Send(new ExportReadingsQuery(deviceId, startDate, endDate));

            if (result.Details is not ExportResultDto exportData)
                throw new BadRequestException("Export data could not be generated.");

            return File(exportData.Content, "text/csv", exportData.FileName);
        }
        public record ExportResultDto(byte[] Content, string FileName);
    }
}