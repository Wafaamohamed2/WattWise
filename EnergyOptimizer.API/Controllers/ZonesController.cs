using EnergyOptimizer.Core.DTOs.ZoneDTOs;
using EnergyOptimizer.Core.Features.Zones.Commands;
using EnergyOptimizer.Core.Features.Zones.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ZonesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ZonesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetZones([FromQuery] int? buildingId)
        {
            var result = await _mediator.Send(new GetZonesForUserQuery(buildingId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateZone([FromBody] CreateZoneDto dto)
        {
            var result = await _mediator.Send(new CreateZoneCommand(dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZone(int id, [FromBody] UpdateZoneDto dto)
        {
            var result = await _mediator.Send(new UpdateZoneCommand(id, dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZone(int id)
        {
            var result = await _mediator.Send(new DeleteZoneCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
