using EnergyOptimizer.Core.Features.Alerts.Commands;
using EnergyOptimizer.Core.Features.Alerts.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AlertsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAlerts([FromQuery] GetAlertsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount([FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetUnreadAlertsCountQuery(buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
           [FromQuery] string? startDate = null,
           [FromQuery] int days = 7,
           [FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new GetAlertStatisticsQuery(startDate, days, buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlert(int id)
        {
            var result = await _mediator.Send(new GetAlertByIdQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _mediator.Send(new MarkAlertAsReadCommand(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("all-read")]
        public async Task<IActionResult> MarkAllAsRead([FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new MarkAllAlertsAsReadCommand(buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var result = await _mediator.Send(new DeleteAlertCommand(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("clear-read")]
        public async Task<IActionResult> ClearReadAlerts([FromQuery] int? buildingId = null)
        {
            var result = await _mediator.Send(new ClearReadAlertsCommand(buildingId));
            return StatusCode(result.StatusCode, result);
        }
    }
}