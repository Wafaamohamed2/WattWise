using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.API.Services;
using Microsoft.AspNetCore.Authorization;
using EnergyOptimizer.Core.Exceptions; 
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
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
            if (_env.IsProduction())
            {
                throw new BadRequestException("Seeding is not allowed in Production environment!");
            }

            await _seedingService.SeedAsync();

            return Ok(new ApiResponse(200, "Data seeded successfully!"));
        }
    }
}