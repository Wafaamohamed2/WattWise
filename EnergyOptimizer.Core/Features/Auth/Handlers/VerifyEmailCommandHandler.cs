using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;

        public VerifyEmailCommandHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task<ApiResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
                throw new BadRequestException("UserId and Token are required.");

            var user = await _identityService.FindUserByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.EmailConfirmed)
                return new ApiResponse(200, "Email is already confirmed.");

            var result = await _identityService.ConfirmEmailAsync(request.UserId, request.Token);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                throw new BadRequestException($"Email verification failed: {errors}");
            }

            return new ApiResponse(200, "Email verified successfully.");
        }
    }
}
