using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new UnauthorizedException("User not found");

            return new ApiResponse(200, "Success", new GetCurrentUserResponseDto(user.Id, user.FullName, user.Email ?? string.Empty));
        }
    }

    public record GetCurrentUserResponseDto(string Id, string FullName, string Email);
}
