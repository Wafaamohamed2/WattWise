using System.Security.Cryptography;
using System.Text;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EnergyOptimizer.Service.Services
{
    public class RefreshTokenService : IRefreshTokenService
    {
        private readonly EnergyDbContext _context;
        private readonly IConfiguration _configuration;

        public RefreshTokenService(EnergyDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string> GenerateRefreshTokenAsync(string userId, string? ipAddress = null)
        {
            var token = GenerateSecureToken();
            var tokenHash = HashToken(token);
            var durationDays = GetDurationInDays();

            var refreshToken = new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = userId,
                ExpiresOn = DateTime.UtcNow.AddDays(durationDays),
                CreatedOn = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return token;
        }

        public async Task<RefreshTokenValidationResult> ValidateRefreshTokenAsync(string token)
        {
            var tokenHash = HashToken(token);
            var storedToken = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken == null)
                throw new UnauthorizedException("Invalid refresh token");

            if (storedToken.IsRevoked)
            {
                await RevokeAllUserTokensAsync(storedToken.UserId);
                throw new UnauthorizedException("Refresh token has been revoked");
            }

            if (storedToken.IsExpired)
                throw new UnauthorizedException("Refresh token has expired");

            return new RefreshTokenValidationResult(storedToken.User, storedToken);
        }

        public async Task<RefreshTokenRotationResult> RotateRefreshTokenAsync(string token, string? ipAddress = null)
        {
            var validation = await ValidateRefreshTokenAsync(token);
            var newToken = GenerateSecureToken();
            var newTokenHash = HashToken(newToken);

            validation.StoredToken.RevokedOn = DateTime.UtcNow;
            validation.StoredToken.ReplacedByToken = newTokenHash;

            var durationDays = GetDurationInDays();
            var newRefreshToken = new RefreshToken
            {
                TokenHash = newTokenHash,
                UserId = validation.User.Id,
                ExpiresOn = DateTime.UtcNow.AddDays(durationDays),
                CreatedOn = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync();

            return new RefreshTokenRotationResult(newToken, validation.User);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var tokenHash = HashToken(token);
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (storedToken == null || storedToken.RevokedOn != null)
                return;

            storedToken.RevokedOn = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserTokensAsync(string userId)
        {
            var activeTokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedOn == null)
                .ToListAsync();

            foreach (var token in activeTokens)
                token.RevokedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hashBytes);
        }

        private int GetDurationInDays()
        {
            var section = _configuration.GetSection("RefreshToken");
            return int.TryParse(section["DurationInDays"], out var days) ? days : 7;
        }
    }
}
