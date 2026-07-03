using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(ApplicationUser user);
    }
}
