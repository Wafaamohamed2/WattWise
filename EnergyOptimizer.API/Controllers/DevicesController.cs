using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
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

        public DevicesController( IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllDevices(
        [FromQuery] GetAllDevicesQuery query)
        {
            var devices = await _mediator.Send(query);
            return Ok(devices);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetDeviceById(int id)
        {
            var result = await _mediator.Send(new GetDeviceByIdQuery(id));
            return result == null ? NotFound(new ApiResponse(404, "Not found")) : Ok(result);

        }

        [HttpGet("zone/{zoneId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetDevicesByZone(int zoneId)
        {
            var devices = await _mediator.Send(new GetDevicesByZoneQuery(zoneId));
            return Ok(devices);            
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateDevice([FromBody] CreateDeviceDto dto)
        {
              var newDevice = await _mediator.Send(new CreateDeviceCommand(dto));
            return Ok(newDevice);
             
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> UpdateDevice(int id, [FromBody] UpdateDeviceDto dto)
        {
            var updatedDevice = await _mediator.Send(new UpdateDeviceCommand(dto));
            return Ok(updatedDevice);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<object>> DeleteDevice(int id)
        {
             var result = await _mediator.Send(new DeleteDeviceCommand(id));
             return Ok(result);
        }

        //  Toggle device status (Active/Inactive) + notify clients via SignalR
        [HttpPatch("{id}/toggle")]
        public async Task<ActionResult> ToggleDevice(int id)
        {
            var ToggleDevice = await _mediator.Send(new ToggleDeviceCommand(id));
            if (ToggleDevice.StatusCode == 404) return NotFound(ToggleDevice);
            return Ok(ToggleDevice);
        }
    }
}