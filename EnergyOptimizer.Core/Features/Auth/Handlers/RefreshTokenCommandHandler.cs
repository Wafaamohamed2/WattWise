using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse>
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IJwtTokenService _jwtTokenService;

        public RefreshTokenCommandHandler(
            IRefreshTokenService refreshTokenService,
            IJwtTokenService jwtTokenService)
        {
            _refreshTokenService = refreshTokenService;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<ApiResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(request.RefreshToken, request.IpAddress);
            var newAccessToken = _jwtTokenService.GenerateToken(rotationResult.User);

            var details = new RefreshTokenResultDetails(newAccessToken, rotationResult.NewRefreshToken);
            return new ApiResponse(200, "Token refreshed successfully", details);
        }
    }

    public record RefreshTokenResultDetails(string Token, string RefreshToken);
}
