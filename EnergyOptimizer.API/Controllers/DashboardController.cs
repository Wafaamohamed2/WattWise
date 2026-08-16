using EnergyOptimizer.Core.Features.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview([FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetDashboardOverviewQuery(buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("consumption-by-zone")]
        public async Task<IActionResult> GetConsumptionByZone([FromQuery] string? startDate = null, [FromQuery] string? endDate = null, [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetConsumptionByZoneQuery(startDate, endDate, buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("consumption-by-device")]
        public async Task<IActionResult> GetConsumptionByDevice([FromQuery] string? startDate = null, [FromQuery] string? endDate = null, [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetConsumptionByDeviceQuery(startDate, endDate, buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("hourly-consumption")]
        public async Task<IActionResult> GetHourlyConsumption([FromQuery] string? date = null, [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetHourlyConsumptionQuery(date, buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("consumption-trend")]
        public async Task<IActionResult> GetConsumptionTrend([FromQuery] int hours = 24, [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetConsumptionTrendQuery(hours, buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("top-consumers")]
        public async Task<IActionResult> GetTopConsumers([FromQuery] int count = 5, [FromQuery] string? startDate = null, [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetTopConsumersQuery(count, startDate, buildingId));
            return StatusCode(result.StatusCode, result);
        }
    }
}