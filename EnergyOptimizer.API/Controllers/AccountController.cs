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

        public AccountController(IMediator mediator, IWebHostEnvironment env)
        {
            _mediator = mediator;
            _env = env;
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

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            DeleteTokenCookie();
            return Ok(new ApiResponse(200, "Logged out successfully"));
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var result = await _mediator.Send(new LoginCommand(model));
            var details = (LoginResultDetails)result.Details!;
            SetTokenCookie(details.Token);
            return Ok(new ApiResponse(200, "Login successful", new { User = details.User }));
        }

        #region Private Helpers

        private void SetTokenCookie(string token)
        {
            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                Path = "/"
            });
        }

        private void DeleteTokenCookie()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
        }

        #endregion
    }
}