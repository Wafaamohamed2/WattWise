using EnergyOptimizer.Core.DTOs.DashboardDTOs;
using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
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

        /// Get dashboard overview statistics

        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetOverview()
        { 
            var result = await _mediator.Send(new GetDashboardOverviewQuery());
            return Ok(result);
        }

        /// <summary>
        /// Get consumption by zone
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd (e.g., 2025-01-14)</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd (e.g., 2025-01-15)</param>
        [HttpGet("consumption-by-zone")]
        public async Task<ActionResult> GetConsumptionByZone([FromQuery] string? startDate = null, [FromQuery] string? endDate = null)
        {
            var consumption = await _mediator.Send(new GetConsumptionByZoneQuery(startDate, endDate));
            return Ok(consumption);

        }

        /// <summary>
        /// Get consumption by device
        /// </summary>
        /// <param name="startDate">Start date in format: yyyy-MM-dd</param>
        /// <param name="endDate">End date in format: yyyy-MM-dd</param>
        [HttpGet("consumption-by-device")]
        public async Task<ActionResult> GetConsumptionByDevice(
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
        {
           
           var consumption = await _mediator.Send(new GetConsumptionByDeviceQuery(startDate, endDate)); 
           return Ok(consumption);
        }


        [HttpGet("hourly-consumption")]
        public async Task<ActionResult> GetHourlyConsumption([FromQuery] string? date = null)
        {
            var result = await _mediator.Send(new GetHourlyConsumptionQuery(date));
            return Ok(result);
        }

        // Get consumption trend for last 24 hours
        [HttpGet("consumption-trend")]
        public async Task<ActionResult<List<ConsumptionTrendDto>>> GetConsumptionTrend(
            [FromQuery] int hours = 24)
        {
            var result = await _mediator.Send(new GetConsumptionTrendQuery(hours));
            return Ok(result);
        }

        [HttpGet("top-consumers")]
        public async Task<ActionResult<List<DeviceConsumptionDto>>> GetTopConsumers(
        [FromQuery] int count = 5,
        [FromQuery] string? startDate = null)
        {
               var result = await _mediator.Send(new GetTopConsumersQuery(count, startDate));
                return Ok(result);
        }

    }
}

    

