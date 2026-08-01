using EnergyOptimizer.Core.DTOs.BuildingDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.BuildingCommands;
using EnergyOptimizer.Core.Features.AI.Queries.BuildingQueries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class BuildingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BuildingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserBuilding()
        {
            var result = await _mediator.Send(new GetUserBuildingQuery());
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingDto dto)
        {
            var result = await _mediator.Send(new CreateBuildingCommand(dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBuilding(int id, [FromBody] UpdateBuildingDto dto)
        {
            var result = await _mediator.Send(new UpdateBuildingCommand(id, dto));
            return StatusCode(result.StatusCode, result);
        }
    }
}
