using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Authorization;
using EnergyOptimizer.Core.Exceptions;
using ApiResponse = EnergyOptimizer.Core.Features.AI.Commands.ApiResponse;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize(Roles ="Admin")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly DataSeedingService _seedingService;
        private readonly IWebHostEnvironment _env;

        public SeedController(DataSeedingService seedingService, IWebHostEnvironment env)
        {
            _seedingService = seedingService;
            _env = env;
        }

        [HttpPost("run")]
        public async Task<IActionResult> SeedData()
        {

            if (!(_env.IsDevelopment() || _env.IsStaging()))
                throw new BadRequestException("Seeding only allowed in Dev/Staging!");

            await _seedingService.SeedAsync();

            return Ok(new ApiResponse(200, "Data seeded successfully!"));
        }
    }
}