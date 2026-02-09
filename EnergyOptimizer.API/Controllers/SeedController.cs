using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.API.Services;
using Microsoft.AspNetCore.Authorization;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly DataSeedingService _seedingService;
        private readonly ILogger<SeedController> _logger;
        private readonly IWebHostEnvironment _env;
        public SeedController(DataSeedingService seedingService, ILogger<SeedController> logger, IWebHostEnvironment env)
        {
            _seedingService = seedingService;
            _logger = logger;
            _env = env;
        }

        [HttpPost("run")]
        public async Task<IActionResult> SeedData()
        {
            if (_env.IsProduction())
            {
                return BadRequest(new ApiResponse(400, "Seeding is not allowed in Production environment!"));
            }

            await _seedingService.SeedAsync();
            return Ok(new ApiResponse(200, "Data seeded successfully!"));
        }
    }
}