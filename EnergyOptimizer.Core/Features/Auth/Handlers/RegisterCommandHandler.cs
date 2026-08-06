using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly IIdentityService _identityService;
        private readonly IGenericRepository<Building>? _buildingRepo;
        private readonly IGenericRepository<Zone>? _zoneRepo;
        private readonly IEmailService? _emailService;
        private readonly IConfiguration? _config;
        private readonly ILogger<RegisterCommandHandler>? _logger;

        public RegisterCommandHandler(
            IIdentityService identityService,
            IEmailService? emailService = null,
            IConfiguration? config = null,
            IGenericRepository<Building>? buildingRepo = null,
            IGenericRepository<Zone>? zoneRepo = null,
            ILogger<RegisterCommandHandler>? logger = null)
        {
            _identityService = identityService;
            _emailService = emailService;
            _config = config;
            _buildingRepo = buildingRepo;
            _zoneRepo = zoneRepo;
            _logger = logger;
        }

        public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var (result, userId) = await _identityService.CreateUserAsync(request.Dto.Email, request.Dto.Password, request.Dto.FullName);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors);
                throw new BadRequestException(errors);
            }

            // Auto-provision default Building and Zone for the new user
            if (_buildingRepo != null)
            {
                try
                {
                    var building = new Building
                    {
                        Name = $"{request.Dto.FullName}'s Smart Home",
                        UserId = userId,
                        Address = "Primary Residence",
                        TotalArea = 150,
                        NumberOfRooms = 4,
                        CreatedAt = DateTime.UtcNow,
                        Zones = new List<Zone>
                        {
                            new Zone
                            {
                                Name = "Living Room",
                                Type = ZoneType.LivingRoom,
                                Area = 35
                            }
                        }
                    };
                    _buildingRepo.Add(building);
                    await _buildingRepo.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to auto-provision default building and zone for user {UserId}", userId);
                }
            }

            if (_emailService != null)
            {
                try
                {
                    var encodedToken = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

                    var frontendUrl = (_config?["FrontendUrl"] ?? "http://127.0.0.1:5500/WattWise-Frontend").TrimEnd('/');
                    var verificationLink = $"{frontendUrl}/verify-email.html?userId={userId}&token={encodedToken}";

                    var emailBody = $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px;'>
                            <h2 style='color: #4f46e5; text-align: center;'>Welcome to WattWise!</h2>
                            <p>Hello {request.Dto.FullName},</p>
                            <p>Thank you for registering. Please confirm your email address by clicking the button below:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{verificationLink}' style='background-color: #4f46e5; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Confirm Email</a>
                            </div>
                            <p>If you did not create this account, no further action is required.</p>
                            <hr style='border: none; border-top: 1px solid #eee; margin: 20px 0;' />
                            <p style='font-size: 12px; color: #888; text-align: center;'>WattWise System</p>
                        </div>";

                    await _emailService.SendEmailAsync(request.Dto.Email, "Confirm your email - WattWise", emailBody);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to send email verification link to {Email} during registration.", request.Dto.Email);

                    return new ApiResponse(200, "User registered successfully, but failed to send the verification email. You can request a new link anytime from the login page.");
                }
            }

            return new ApiResponse(200, "User registered successfully! Please check your email to verify your account.");
        }
    }
}
