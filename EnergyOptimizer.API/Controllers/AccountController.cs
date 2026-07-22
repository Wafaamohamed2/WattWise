using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Features.Auth.Queries;
using EnergyOptimizer.Core.Features.Auth.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using static EnergyOptimizer.Core.DTOs.AuthDto;

namespace EnergyOptimizer.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public AccountController(IMediator mediator, IWebHostEnvironment env, IConfiguration config)
        {
            _mediator = mediator;
            _env = env;
            _config = config;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var result = await _mediator.Send(new RegisterCommand(model));
            return Ok(result);
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _mediator.Send(new GetCurrentUserQuery(userId));
            return Ok(result);
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var ipAddress = GetIpAddress();
            var result = await _mediator.Send(new LoginCommand(model, ipAddress));
            var details = (LoginResultDetails)result.Details!;

            SetAccessTokenCookie(details.Token);
            SetRefreshTokenCookie(details.RefreshToken);

            return Ok(new ApiResponse(200, "Login successful", new { User = details.User }));
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? dto = null)
        {
            var refreshToken = Request.Cookies["refresh_token"] ?? dto?.RefreshToken;

            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new BadRequestException("Refresh token is required");

            var ipAddress = GetIpAddress();
            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken, ipAddress));
            var details = (RefreshTokenResultDetails)result.Details!;

            SetAccessTokenCookie(details.Token);
            SetRefreshTokenCookie(details.RefreshToken);

            return Ok(new ApiResponse(200, "Token refreshed successfully"));
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto? dto = null)
        {
            var refreshToken = Request.Cookies["refresh_token"] ?? dto?.RefreshToken;

            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _mediator.Send(new RevokeTokenCommand(refreshToken));
            }

            DeleteTokenCookies();
            return Ok(new ApiResponse(200, "Logged out successfully"));
        }

        #region Private Helpers

        private void SetAccessTokenCookie(string token)
        {
            var durationMinutes = int.TryParse(_config["Jwt:DurationInMinutes"], out var minutes) ? minutes : 15;
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddMinutes(durationMinutes),
                Path = "/"
            });
        }

        private void SetRefreshTokenCookie(string refreshToken)
        {
            var durationDays = int.TryParse(_config["RefreshToken:DurationInDays"], out var days) ? days : 7;
            Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddDays(durationDays),
                Path = "/"
            });
        }

        private void DeleteTokenCookies()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Path = "/"
            };

            Response.Cookies.Delete("access_token", cookieOptions);
            Response.Cookies.Delete("refresh_token", cookieOptions);
        }

        private string? GetIpAddress()
        {
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                return forwardedFor.FirstOrDefault();

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        #endregion
    }

    public record RefreshTokenRequestDto(string? RefreshToken);
}