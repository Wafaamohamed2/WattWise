using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Queries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse>
    {
        private readonly IIdentityService _identityService;

        public GetCurrentUserQueryHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<ApiResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindUserByIdAsync(request.UserId);
            if (user == null)
                throw new UnauthorizedException("User not found");

            return new ApiResponse(200, "Success", new GetCurrentUserResponseDto(user.Id, user.FullName, user.Email));
        }
    }

    public record GetCurrentUserResponseDto(string Id, string FullName, string Email);
}
