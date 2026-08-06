using EnergyOptimizer.Core.DTOs.AuthDTOs;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IIdentityService
    {
        Task<(IdentityResultDto Result, string UserId)> CreateUserAsync(string email, string password, string fullName);
        Task<(bool IsValid, UserAuthInfo? User)> ValidateUserCredentialsAsync(string email, string password);
        Task<UserAuthInfo?> FindUserByEmailAsync(string email);
        Task<UserAuthInfo?> FindUserByIdAsync(string userId);
        Task<string> GenerateEmailConfirmationTokenAsync(string userId);
        Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token);
        Task<string?> GeneratePasswordResetTokenAsync(string email);
        Task<IdentityResultDto> ResetPasswordAsync(string email, string token, string newPassword);
        Task<IdentityResultDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
