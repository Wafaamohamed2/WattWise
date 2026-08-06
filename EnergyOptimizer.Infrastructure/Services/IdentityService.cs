using System.Text;
using EnergyOptimizer.Core.DTOs.AuthDTOs;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Data.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace EnergyOptimizer.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(IdentityResultDto Result, string UserId)> CreateUserAsync(string email, string password, string fullName)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (IdentityResultDto.Failure(result.Errors.Select(e => e.Description)), string.Empty);
            }

            return (IdentityResultDto.Success(), user.Id);
        }

        public async Task<(bool IsValid, UserAuthInfo? User)> ValidateUserCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, null);

            var isValid = await _userManager.CheckPasswordAsync(user, password);
            if (!isValid)
                return (false, null);

            var userAuthInfo = new UserAuthInfo(user.Id, user.Email!, user.FullName, user.EmailConfirmed);
            return (true, userAuthInfo);
        }

        public async Task<UserAuthInfo?> FindUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            return new UserAuthInfo(user.Id, user.Email!, user.FullName, user.EmailConfirmed);
        }

        public async Task<UserAuthInfo?> FindUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return new UserAuthInfo(user.Id, user.Email!, user.FullName, user.EmailConfirmed);
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new KeyNotFoundException($"User with ID '{userId}' was not found.");

            var rawToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        }

        public async Task<IdentityResultDto> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResultDto.Failure("User not found");

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch
            {
                decodedToken = token;
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
            }

            return IdentityResultDto.Success();
        }

        public async Task<string?> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return null;

            var rawToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));
        }

        public async Task<IdentityResultDto> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return IdentityResultDto.Failure("User not found");

            string decodedToken;
            try
            {
                decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            }
            catch
            {
                decodedToken = token;
            }

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);
            if (!result.Succeeded)
            {
                return IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
            }

            return IdentityResultDto.Success();
        }

        public async Task<IdentityResultDto> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResultDto.Failure("User not found");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                return IdentityResultDto.Failure(result.Errors.Select(e => e.Description));
            }

            return IdentityResultDto.Success();
        }
    }
}
