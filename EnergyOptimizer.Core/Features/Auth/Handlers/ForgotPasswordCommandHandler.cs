using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<ForgotPasswordCommandHandler>? _logger;

        public ForgotPasswordCommandHandler(
            IIdentityService identityService,
            IEmailService emailService,
            IConfiguration config,
            ILogger<ForgotPasswordCommandHandler>? logger = null)
        {
            _identityService = identityService;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            const string genericMessage = "If an account with that email exists, a password reset link has been sent.";

            if (string.IsNullOrWhiteSpace(request.Email))
                return new ApiResponse(200, genericMessage);

            var user = await _identityService.FindUserByEmailAsync(request.Email);
            if (user == null)
            {
                return new ApiResponse(200, genericMessage);
            }

            try
            {
                var encodedToken = await _identityService.GeneratePasswordResetTokenAsync(request.Email);
                if (encodedToken != null)
                {
                    var frontendUrl = (_config["FrontendUrl"] ?? "http://127.0.0.1:5500/WattWise-Frontend").TrimEnd('/');
                    var resetLink = $"{frontendUrl}/reset-password.html?email={Uri.EscapeDataString(user.Email)}&token={encodedToken}";

                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                            <h2 style='color: #4f46e5; text-align: center;'>WattWise Password Reset</h2>
                            <p>Hello {user.FullName},</p>
                            <p>We received a request to reset your password. Click the button below to choose a new password:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Reset Password</a>
                            </div>
                            <p>If you did not request a password reset, please ignore this email.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #888; text-align: center;'>WattWise System</p>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email, "Reset your password - WattWise", emailBody);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to send password reset email to {Email}", request.Email);
            }

            return new ApiResponse(200, genericMessage);
        }
    }
}
