using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
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
            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var result = await _mediator.Send(new GetUnreadAlertsCountQuery());
            return Ok(result);
        }

        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics(
           [FromQuery] string? startDate = null,
           [FromQuery] int days = 7)
        {
            var result = await _mediator.Send(new GetAlertStatisticsQuery(startDate, days));
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAlert(int id)
        {
            var result = await _mediator.Send(new GetAlertByIdQuery(id));
            return Ok(result);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _mediator.Send(new MarkAlertAsReadCommand(id));
            return Ok(result);
        }

        [HttpPost("all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _mediator.Send(new MarkAllAlertsAsReadCommand());
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAlert(int id)
        {
            var result = await _mediator.Send(new DeleteAlertCommand(id));
            return Ok(result);
        }

        [HttpDelete("clear-read")]
        public async Task<IActionResult> ClearReadAlerts()
        {
            var result = await _mediator.Send(new ClearReadAlertsCommand());
            return Ok(result);
        }
    }
}