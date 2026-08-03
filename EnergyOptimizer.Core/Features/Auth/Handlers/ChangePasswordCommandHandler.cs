using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRefreshTokenService _refreshTokenService;

        public ChangePasswordCommandHandler(
            UserManager<ApplicationUser> userManager,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Failed to change password: {errors}");
            }

            await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

            return new ApiResponse(200, "Password changed successfully.");
        }
    }
}
