using MediatR;
using Microsoft.Extensions.Configuration;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class ResendConfirmationEmailCommandHandler : IRequestHandler<ResendConfirmationEmailCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public ResendConfirmationEmailCommandHandler(
            IIdentityService identityService,
            IEmailService emailService,
            IConfiguration config)
        {
            _identityService = identityService;
            _emailService = emailService;
            _config = config;
        }

        public async Task<ApiResponse> Handle(ResendConfirmationEmailCommand request, CancellationToken cancellationToken)
        {
            const string genericMessage = "If an account with that email exists, a verification link has been sent.";

            if (string.IsNullOrWhiteSpace(request.Email))
                return new ApiResponse(200, genericMessage);

            var user = await _identityService.FindUserByEmailAsync(request.Email);
            if (user == null || user.EmailConfirmed)
            {
                return new ApiResponse(200, genericMessage);
            }

            var encodedToken = await _identityService.GenerateEmailConfirmationTokenAsync(user.Id);

            var frontendUrl = (_config["FrontendUrl"] ?? "http://127.0.0.1:5500/WattWise-Frontend").TrimEnd('/');
            var verificationLink = $"{frontendUrl}/verify-email.html?userId={user.Id}&token={encodedToken}";

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                    <h2 style='color: #4f46e5; text-align: center;'>WattWise</h2>
                    <p>Hello {user.FullName},</p>
                    <p>You requested a new email verification link. Please confirm your email address by clicking the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{verificationLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Verify Email Address</a>
                    </div>
                    <p>If you did not request this email, please ignore it.</p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                    <p style='font-size: 12px; color: #888; text-align: center;'>WattWise System</p>
                </div>";

            await _emailService.SendEmailAsync(user.Email, "Confirm your Email - WattWise", emailBody);

            return new ApiResponse(200, genericMessage);
        }
    }
}
