using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokenService _refreshTokenService;

        public ChangePasswordCommandHandler(
            IIdentityService identityService,
            IRefreshTokenService refreshTokenService)
        {
            _identityService = identityService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<ApiResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindUserByIdAsync(request.UserId);
            if (user == null)
            {
                throw new NotFoundException("User not found");
            }

            var result = await _identityService.ChangePasswordAsync(request.UserId, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                throw new BadRequestException($"Failed to change password: {errors}");
            }

            await _refreshTokenService.RevokeAllUserTokensAsync(user.Id);

            return new ApiResponse(200, "Password changed successfully.");
        }
    }
}
