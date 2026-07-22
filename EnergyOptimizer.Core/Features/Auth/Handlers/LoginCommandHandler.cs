using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService tokenService,
            IRefreshTokenService refreshTokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<ApiResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Dto.Email);

            if (user == null)
                throw new BadRequestException("Invalid email or password");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Dto.Password);

            if (!isPasswordValid)
                throw new UnauthorizedException("Invalid email or password");

            var token = _tokenService.GenerateToken(user);
            var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(user.Id, request.IpAddress);

            var details = new LoginResultDetails(token, refreshToken, new LoginUserDto(user.Id, user.FullName, user.Email ?? string.Empty));

            return new ApiResponse(200, "Login successful", details);
        }
    }

    public record LoginResultDetails(string Token, string RefreshToken, LoginUserDto User);
    public record LoginUserDto(string Id, string FullName, string Email);
}
