using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, ApiResponse>
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public RevokeTokenCommandHandler(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        public async Task<ApiResponse> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            await _refreshTokenService.RevokeRefreshTokenAsync(request.RefreshToken);
            return new ApiResponse(200, "Token revoked successfully");
        }
    }
}
