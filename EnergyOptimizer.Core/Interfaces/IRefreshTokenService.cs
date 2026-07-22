using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateRefreshTokenAsync(string userId, string? ipAddress = null);
        Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token);
        Task<RefreshTokenRotationResult> RotateRefreshTokenAsync(string token, string? ipAddress = null);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(string userId);
    }

    public record RefreshTokenValidationResult(ApplicationUser User, RefreshToken StoredToken);
    public record RefreshTokenRotationResult(string NewRefreshToken, ApplicationUser User);
}
