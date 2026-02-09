using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static EnergyOptimizer.API.DTOs.AuthDto;

namespace EnergyOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto(false, "Invalid input data"));

            var user = new ApplicationUser
            {
                UserName = model.Email, 
                Email = model.Email,
                FullName = model.FullName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            
            if (!result.Succeeded)
                return BadRequest(new AuthResponseDto(false, string.Join(", ", result.Errors.Select(e => e.Description))));

            return Ok(new AuthResponseDto(true, "User registered successfully!"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (!ModelState.IsValid)
                return BadRequest(new AuthResponseDto(false, "Invalid input data"));

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            
            if (!isPasswordValid)
                return Unauthorized(new AuthResponseDto(false, "Invalid email or password"));

            var token = GenerateJwtToken(user);
            return Ok(new ApiResponse(200, "Login successful", new { Token = token, User = user }));
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim("FullName", user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["DurationInMinutes"]!)),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}