using AutoMapper;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using static EnergyOptimizer.API.DTOs.AuthDto;
using static EnergyOptimizer.API.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IJwtTokenService tokenService,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
            _env = env;
        }

        [HttpPost("register")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                throw new BadRequestException("Invalid input data");

            var user = _mapper.Map<ApplicationUser>(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            return Ok(new ApiResponse(200, "User registered successfully!"));
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                throw new UnauthorizedException("Not authenticated");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedException("User not found");

            return Ok(new ApiResponse(200, "Success", new { user.Id, user.FullName, user.Email }));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("access_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Strict,
                Path = "/"
            });
            return Ok(new ApiResponse(200, "Logged out successfully"));
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                throw new BadRequestException("Invalid email or password");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isPasswordValid)
                throw new UnauthorizedException("Invalid email or password");

            var token = _tokenService.GenerateToken(user);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(8),
                Path = "/"
            });

            return Ok(new ApiResponse(200, "Login successful", new
            {
                User = new { user.Id, user.FullName, user.Email }
            }));
        }
    }
}