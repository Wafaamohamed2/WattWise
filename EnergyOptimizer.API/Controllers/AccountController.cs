using AutoMapper;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static EnergyOptimizer.API.DTOs.AuthDto;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountController(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IJwtTokenService tokenService) 
        {
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
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

            return Ok(new ApiResponse(200, "Login successful", new
            {
                Token = token,
                User = new { user.Id, user.FullName, user.Email }
            }));
        }

    }
}