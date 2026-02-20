using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("overview")]
        public async Task<IActionResult> GetOverview()
        {
            var result = await _mediator.Send(new GetDashboardOverviewQuery());
            return Ok(result);
        }

        [HttpGet("consumption-by-zone")]
        public async Task<IActionResult> GetConsumptionByZone([FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var result = await _mediator.Send(new GetConsumptionByZoneQuery(startDate, endDate));
            return Ok(result);
        }

        [HttpGet("consumption-by-device")]
        public async Task<IActionResult> GetConsumptionByDevice([FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var result = await _mediator.Send(new GetConsumptionByDeviceQuery(startDate, endDate));
            return Ok(result);
        }

        [HttpGet("hourly-consumption")]
        public async Task<IActionResult> GetHourlyConsumption([FromQuery] string? date = null)
        {
            var result = await _mediator.Send(new GetHourlyConsumptionQuery(date));
            return Ok(result);
        }

        [HttpGet("consumption-trend")]
        public async Task<IActionResult> GetConsumptionTrend([FromQuery] int hours = 24)
        {
            var result = await _mediator.Send(new GetConsumptionTrendQuery(hours));
            return Ok(result);
        }

        [HttpGet("top-consumers")]
        public async Task<IActionResult> GetTopConsumers([FromQuery] int count = 5, [FromQuery] string? startDate = null)
        {
            var result = await _mediator.Send(new GetTopConsumersQuery(count, startDate));
            return Ok(result);
        }
    }
}