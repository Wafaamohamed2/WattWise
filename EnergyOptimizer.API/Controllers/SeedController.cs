using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.API.Services;
using Microsoft.AspNetCore.Authorization;
using EnergyOptimizer.Core.Exceptions; 
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize(Roles ="Admin")]
    [ApiController]
    [Route("api/[controller]")]
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