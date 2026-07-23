using System.Text;
using AutoMapper;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailService? _emailService;
        private readonly IConfiguration? _config;
        private readonly ILogger<RegisterCommandHandler>? _logger;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IEmailService? emailService = null,
            IConfiguration? config = null,
            ILogger<RegisterCommandHandler>? logger = null)
        {
            _userManager = userManager;
            _mapper = mapper;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<ApplicationUser>(request.Dto);

            var result = await _userManager.CreateAsync(user, request.Dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            if (_emailService != null)
            {
                try
                {
                    var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));

                    var frontendUrl = (_config?["FrontendUrl"] ?? "http://127.0.0.1:5500/WattWise-Frontend").TrimEnd('/');
                    var verificationLink = $"{frontendUrl}/verify-email.html?userId={user.Id}&token={encodedToken}";

                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                            <h2 style='color: #4f46e5; text-align: center;'>Welcome to WattWise!</h2>
                            <p>Hello {user.FullName},</p>
                            <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Confirm Email</a>
                            </div>
                            <p>If you did not create this account, no further action is required.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #888; text-align: center;'>WattWise System</p>
                        </div>";

                    await _emailService.SendEmailAsync(user.Email!, "Confirm your email - WattWise", emailBody);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to send email verification link to {Email} during registration.", user.Email);

                    return new ApiResponse(200, "User registered successfully, but failed to send the verification email. You can request a new link anytime from the login page.");
                }
            }

            return new ApiResponse(200, "User registered successfully! Please check your email to verify your account.");
        }
    }
}
