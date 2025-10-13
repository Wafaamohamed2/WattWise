using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.API.Services;

namespace EnergyOptimizer.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly DataSeedingService _seedingService;
        private readonly ILogger<SeedController> _logger;

        public SeedController(DataSeedingService seedingService, ILogger<SeedController> logger)
        {
            _seedingService = seedingService;
            _logger = logger;
        }

        [HttpPost("run")]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                await _seedingService.SeedAsync();
                return Ok(new { message = "Data seeded successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to seed data");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}