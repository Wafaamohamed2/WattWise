using System.Text;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyEmailCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ApiResponse> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId) || string.IsNullOrWhiteSpace(request.Token))
                throw new BadRequestException("UserId and Token are required.");

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new NotFoundException("User not found.");

            if (user.EmailConfirmed)
                return new ApiResponse(200, "Email is already confirmed.");

            string decodedToken;
            try
            {
                var decodedBytes = WebEncoders.Base64UrlDecode(request.Token);
                decodedToken = Encoding.UTF8.GetString(decodedBytes);
            }
            catch (Exception)
            {
                throw new BadRequestException("Invalid token format.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException($"Email verification failed: {errors}");
            }

            return new ApiResponse(200, "Email verified successfully.");
        }
    }
}
