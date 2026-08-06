using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokenService _refreshTokenService;

        public ResetPasswordCommandHandler(
            IIdentityService identityService,
            IRefreshTokenService refreshTokenService)
        {
            _identityService = identityService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<ApiResponse> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new BadRequestException("Invalid request");
            }

            var result = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                throw new BadRequestException($"Failed to reset password: {errors}");
            }

            await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

            return new ApiResponse(200, "Password reset successfully. You can now login with your new password.");
        }
    }
}
