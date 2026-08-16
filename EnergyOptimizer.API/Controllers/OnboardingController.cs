using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.DTOs.OnboardingDTOs;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.Onboarding.Commands;
using EnergyOptimizer.Core.Features.Onboarding.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class OnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OnboardingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("building")]
        public async Task<IActionResult> CreateBuilding([FromBody] CreateBuildingWithTypeDto dto)
        {
            var result = await _mediator.Send(new CreateBuildingWithTypeCommand(dto));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] BuildingType buildingType = BuildingType.Home)
        {
            var result = await _mediator.Send(new GetDeviceTemplatesQuery(buildingType));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
        {
            var result = await _mediator.Send(new CompleteOnboardingCommand(dto));
            return StatusCode(result.StatusCode, result);
        }
    }
}
