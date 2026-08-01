using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EnergyOptimizer.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId =>
            _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string RequireUserId()
        {
            var userId = UserId;
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedException("User identity not found in request context.");
            }
            return userId;
        }
    }
}
