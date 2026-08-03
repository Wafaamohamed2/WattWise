using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Features.Devices.Commands;
using EnergyOptimizer.Core.Features.Devices.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeviceById(int id)
        {
            var result = await _mediator.Send(new GetDeviceByIdQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("zone/{zoneId}")]
        public async Task<IActionResult> GetDevicesByZone(int zoneId)
        {
            var result = await _mediator.Send(new GetDevicesByZoneQuery(zoneId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceDto dto)
        {
            var result = await _mediator.Send(new CreateDeviceCommand(dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            var result = await _mediator.Send(new UpdateDeviceCommand(id, dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(int id)
        {
            var result = await _mediator.Send(new DeleteDeviceCommand(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleDevice(int id)
        {
            var result = await _mediator.Send(new ToggleDeviceCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}