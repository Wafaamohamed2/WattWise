using EnergyOptimizer.Core.DTOs.AuthDTOs;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserAuthInfo user);
    }
}
