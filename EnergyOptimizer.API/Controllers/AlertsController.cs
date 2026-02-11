using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AlertsController(IMediator mediator) => _mediator = mediator;

        // Get all alerts with filters
        [HttpGet]
        public async Task<ActionResult<object>> GetAlerts(
            [FromQuery] GetAlertsQuery query)
        {
            var alerts = await _mediator.Send(query);
            return Ok(alerts);
        }

        // Get unread alerts count
        [HttpGet("unread-count")]
        public async Task<ActionResult<object>> GetUnreadCount()
        {
                var result = await _mediator.Send(new GetUnreadAlertsCountQuery());
                return Ok(result);
        }

        // Get alerts statistics
        [HttpGet("statistics")]
        public async Task<ActionResult<AlertStatistics>> GetStatistics(
           [FromQuery] string? startDate = null,
           [FromQuery] int days = 7)
        {
             var result = await _mediator.Send(new GetAlertStatisticsQuery(startDate, days));
            return Ok(result);
        }

        // Get alert by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<AlertDto>> GetAlert(int id)
        {
            var result = await _mediator.Send(new GetAlertByIdQuery(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        // Mark alert as read
        [HttpPatch("{id}/read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var result = await _mediator.Send(new MarkAlertAsReadCommand (id) );
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        // Mark all alerts as read
        [HttpPost("all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var result = await _mediator.Send(new MarkAllAlertsAsReadCommand());
            return Ok(result);
        }

        // Delete alert 
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAlert(int id)
        {
            var result = await _mediator.Send(new DeleteAlertCommand(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        // Delete all read alerts
        [HttpDelete("clear-read")]
        public async Task<ActionResult> ClearReadAlerts()
        {
            var result = await _mediator.Send(new ClearReadAlertsCommand());
            return Ok(result);
        }
    }
}
