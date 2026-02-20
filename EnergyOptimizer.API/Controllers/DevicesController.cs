using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DevicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDevices([FromQuery] GetAllDevicesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(int id)
        {
            var result = await _mediator.Send(new GetDeviceByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("zone/{zoneId}")]
        public async Task<IActionResult> GetDevicesByZone(int zoneId)
        {
            var result = await _mediator.Send(new GetDevicesByZoneQuery(zoneId));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto dto)
        {
            var result = await _mediator.Send(new CreateDeviceCommand(dto));
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            var result = await _mediator.Send(new UpdateDeviceCommand(dto));
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var result = await _mediator.Send(new DeleteDeviceCommand(id));
            return Ok(result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleDevice(int id)
        {
            var result = await _mediator.Send(new ToggleDeviceCommand(id));
            return Ok(result);
        }
    }
}