using EnergyOptimizer.Core.DTOs.AuthDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Features.Auth.Handlers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Moq;
using static EnergyOptimizer.Core.DTOs.AuthDto;

namespace EnergyOptimizer.Tests.Handlers.Auth
{
    public class AuthHandlersTests
    {
        private readonly Mock<IIdentityService> _mockIdentityService;
        private readonly Mock<IJwtTokenService> _mockTokenService;
        private readonly Mock<IRefreshTokenService> _mockRefreshTokenService;

        public AuthHandlersTests()
        {
            _mockIdentityService = new Mock<IIdentityService>();
            _mockTokenService = new Mock<IJwtTokenService>();
            _mockRefreshTokenService = new Mock<IRefreshTokenService>();
        }

        [Fact]
        public async Task RegisterHandler_ValidData_CreatesUser()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);

            _mockIdentityService.Setup(i => i.CreateUserAsync(dto.Email, dto.Password, dto.FullName))
                .ReturnsAsync((IdentityResultDto.Success(), "user-123"));

            var handler = new RegisterCommandHandler(_mockIdentityService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("User registered successfully! Please check your email to verify your account.");
        }

        [Fact]
        public async Task RegisterHandler_ValidData_AutoProvisionsBuildingAndZone()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);

            var mockBuildingRepo = new Mock<IGenericRepository<Building>>();
            var mockZoneRepo = new Mock<IGenericRepository<Zone>>();

            _mockIdentityService.Setup(i => i.CreateUserAsync(dto.Email, dto.Password, dto.FullName))
                .ReturnsAsync((IdentityResultDto.Success(), "user-123"));

            var handler = new RegisterCommandHandler(
                _mockIdentityService.Object,
                emailService: null,
                config: null,
                buildingRepo: mockBuildingRepo.Object,
                zoneRepo: mockZoneRepo.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            mockBuildingRepo.Verify(repo => repo.Add(It.Is<Building>(bld => bld.UserId == "user-123" && bld.Zones.Count == 1)), Times.Once);
            mockBuildingRepo.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task RegisterHandler_FailedCreation_ThrowsBadRequest()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);

            _mockIdentityService.Setup(i => i.CreateUserAsync(dto.Email, dto.Password, dto.FullName))
                .ReturnsAsync((IdentityResultDto.Failure("Email taken"), string.Empty));

            var handler = new RegisterCommandHandler(_mockIdentityService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Email taken");
        }

        [Fact]
        public async Task RegisterHandler_EmailServiceFails_ReturnsWarningMessage()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);
            var mockEmailService = new Mock<IEmailService>();

            _mockIdentityService.Setup(i => i.CreateUserAsync(dto.Email, dto.Password, dto.FullName))
                .ReturnsAsync((IdentityResultDto.Success(), "user-1"));
            _mockIdentityService.Setup(i => i.GenerateEmailConfirmationTokenAsync("user-1"))
                .ReturnsAsync("token");

            mockEmailService.Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                            .ThrowsAsync(new Exception("SMTP server down"));

            var handler = new RegisterCommandHandler(_mockIdentityService.Object, mockEmailService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Contain("failed to send the verification email");
        }

        [Fact]
        public async Task LoginHandler_ValidCredentials_ReturnsTokensAndUser()
        {
            // Arrange
            var dto = new LoginDto("test@example.com", "Password123!");
            var command = new LoginCommand(dto, "127.0.0.1");
            var userAuthInfo = new UserAuthInfo("1", dto.Email, "Ali Mohamed", true);

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(dto.Email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ValidateUserCredentialsAsync(dto.Email, dto.Password))
                .ReturnsAsync((true, userAuthInfo));

            _mockTokenService.Setup(t => t.GenerateToken(userAuthInfo)).Returns("fake-jwt-token");
            _mockRefreshTokenService.Setup(r => r.GenerateRefreshTokenAsync(userAuthInfo.Id, "127.0.0.1")).ReturnsAsync("fake-refresh-token");

            var handler = new LoginCommandHandler(_mockIdentityService.Object, _mockTokenService.Object, _mockRefreshTokenService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var details = result.Details.Should().BeOfType<LoginResultDetails>().Subject;
            details.Token.Should().Be("fake-jwt-token");
            details.RefreshToken.Should().Be("fake-refresh-token");
            details.User.Id.Should().Be("1");
            details.User.FullName.Should().Be("Ali Mohamed");
        }

        [Fact]
        public async Task RefreshTokenHandler_ValidToken_ReturnsNewTokens()
        {
            // Arrange
            var userAuthInfo = new UserAuthInfo("1", "test@example.com", "Ali Mohamed", true);
            var command = new RefreshTokenCommand("old-refresh-token", "127.0.0.1");
            var rotationResult = new RefreshTokenRotationResult("new-refresh-token", userAuthInfo);

            _mockRefreshTokenService.Setup(r => r.RotateRefreshTokenAsync("old-refresh-token", "127.0.0.1"))
                                    .ReturnsAsync(rotationResult);
            _mockTokenService.Setup(t => t.GenerateToken(userAuthInfo)).Returns("new-access-token");

            var handler = new RefreshTokenCommandHandler(_mockRefreshTokenService.Object, _mockTokenService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var details = result.Details.Should().BeOfType<RefreshTokenResultDetails>().Subject;
            details.Token.Should().Be("new-access-token");
            details.RefreshToken.Should().Be("new-refresh-token");
        }

        [Fact]
        public async Task LoginHandler_UnconfirmedEmail_ThrowsUnauthorized()
        {
            // Arrange
            var dto = new LoginDto("unconfirmed@example.com", "Password123!");
            var command = new LoginCommand(dto, "127.0.0.1");
            var userAuthInfo = new UserAuthInfo("1", dto.Email, "Ali Mohamed", false);

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(dto.Email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ValidateUserCredentialsAsync(dto.Email, dto.Password))
                .ReturnsAsync((true, userAuthInfo));

            var handler = new LoginCommandHandler(_mockIdentityService.Object, _mockTokenService.Object, _mockRefreshTokenService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>()
                .WithMessage("Please confirm your email address before logging in.");
        }

        [Fact]
        public async Task VerifyEmailHandler_ValidToken_ConfirmsEmailSuccessfully()
        {
            // Arrange
            var userAuthInfo = new UserAuthInfo("user-123", "test@example.com", "Wafaa Mohamed", false);
            var command = new VerifyEmailCommand(userAuthInfo.Id, "sample-token");

            _mockIdentityService.Setup(i => i.FindUserByIdAsync(userAuthInfo.Id)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ConfirmEmailAsync(userAuthInfo.Id, "sample-token")).ReturnsAsync(IdentityResultDto.Success());

            var handler = new VerifyEmailCommandHandler(_mockIdentityService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Email verified successfully.");
        }

        [Fact]
        public async Task VerifyEmailHandler_InvalidUser_ThrowsNotFoundException()
        {
            // Arrange
            var command = new VerifyEmailCommand("invalid-id", "some-token");
            _mockIdentityService.Setup(i => i.FindUserByIdAsync("invalid-id")).ReturnsAsync((UserAuthInfo?)null);

            var handler = new VerifyEmailCommandHandler(_mockIdentityService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>().WithMessage("User not found.");
        }

        [Fact]
        public async Task ResendConfirmationEmailHandler_UnconfirmedUser_SendsEmailAndReturnsGenericMessage()
        {
            // Arrange
            var email = "unconfirmed@example.com";
            var userAuthInfo = new UserAuthInfo("user-1", email, "Test User", false);
            var mockEmailService = new Mock<IEmailService>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.GenerateEmailConfirmationTokenAsync("user-1")).ReturnsAsync("new-token");

            var handler = new ResendConfirmationEmailCommandHandler(_mockIdentityService.Object, mockEmailService.Object, mockConfig.Object);

            // Act
            var result = await handler.Handle(new ResendConfirmationEmailCommand(email), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("If an account with that email exists, a verification link has been sent.");
            mockEmailService.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ResendConfirmationEmailHandler_AlreadyConfirmedUser_ReturnsGenericMessageWithoutSendingEmail()
        {
            // Arrange
            var email = "confirmed@example.com";
            var userAuthInfo = new UserAuthInfo("user-2", email, "Test User", true);
            var mockEmailService = new Mock<IEmailService>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync(userAuthInfo);

            var handler = new ResendConfirmationEmailCommandHandler(_mockIdentityService.Object, mockEmailService.Object, mockConfig.Object);

            // Act
            var result = await handler.Handle(new ResendConfirmationEmailCommand(email), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("If an account with that email exists, a verification link has been sent.");
            mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ForgotPasswordHandler_ExistingUser_SendsEmailAndReturnsGenericMessage()
        {
            // Arrange
            var email = "forgot@example.com";
            var userAuthInfo = new UserAuthInfo("user-10", email, "Forgot User", true);
            var mockEmailService = new Mock<IEmailService>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.GeneratePasswordResetTokenAsync(email)).ReturnsAsync("reset-token-123");

            var handler = new ForgotPasswordCommandHandler(_mockIdentityService.Object, mockEmailService.Object, mockConfig.Object);

            // Act
            var result = await handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("If an account with that email exists, a password reset link has been sent.");
            mockEmailService.Verify(e => e.SendEmailAsync(email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ForgotPasswordHandler_NonExistingUser_ReturnsGenericMessageWithoutSendingEmail()
        {
            // Arrange
            var email = "nonexisting@example.com";
            var mockEmailService = new Mock<IEmailService>();
            var mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync((UserAuthInfo?)null);

            var handler = new ForgotPasswordCommandHandler(_mockIdentityService.Object, mockEmailService.Object, mockConfig.Object);

            // Act
            var result = await handler.Handle(new ForgotPasswordCommand(email), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("If an account with that email exists, a password reset link has been sent.");
            mockEmailService.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ResetPasswordHandler_ValidToken_ResetsPasswordAndRevokesTokens()
        {
            // Arrange
            var email = "reset@example.com";
            var userAuthInfo = new UserAuthInfo("user-20", email, "Reset User", true);
            var token = "valid-token";
            var newPassword = "NewPassword123!";

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ResetPasswordAsync(email, token, newPassword)).ReturnsAsync(IdentityResultDto.Success());

            var handler = new ResetPasswordCommandHandler(_mockIdentityService.Object, _mockRefreshTokenService.Object);

            // Act
            var result = await handler.Handle(new ResetPasswordCommand(email, token, newPassword), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Password reset successfully. You can now login with your new password.");
            _mockRefreshTokenService.Verify(r => r.RevokeAllUserTokensAsync(userAuthInfo.Id), Times.Once);
        }

        [Fact]
        public async Task ResetPasswordHandler_FailedReset_ThrowsBadRequestException()
        {
            // Arrange
            var email = "reset@example.com";
            var userAuthInfo = new UserAuthInfo("user-20", email, "Reset User", true);
            var token = "invalid-token";
            var newPassword = "NewPassword123!";

            _mockIdentityService.Setup(i => i.FindUserByEmailAsync(email)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ResetPasswordAsync(email, token, newPassword))
                .ReturnsAsync(IdentityResultDto.Failure("Invalid token"));

            var handler = new ResetPasswordCommandHandler(_mockIdentityService.Object, _mockRefreshTokenService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(new ResetPasswordCommand(email, token, newPassword), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Failed to reset password*");
        }

        [Fact]
        public async Task ChangePasswordHandler_ValidPasswords_ChangesPasswordAndRevokesTokens()
        {
            // Arrange
            var userId = "user-30";
            var userAuthInfo = new UserAuthInfo(userId, "test@example.com", "Test User", true);
            var currentPassword = "OldPassword123!";
            var newPassword = "NewPassword123!";

            _mockIdentityService.Setup(i => i.FindUserByIdAsync(userId)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ChangePasswordAsync(userId, currentPassword, newPassword)).ReturnsAsync(IdentityResultDto.Success());

            var handler = new ChangePasswordCommandHandler(_mockIdentityService.Object, _mockRefreshTokenService.Object);

            // Act
            var result = await handler.Handle(new ChangePasswordCommand(userId, currentPassword, newPassword), CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("Password changed successfully.");
            _mockRefreshTokenService.Verify(r => r.RevokeAllUserTokensAsync(userId), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordHandler_IncorrectCurrentPassword_ThrowsBadRequestException()
        {
            // Arrange
            var userId = "user-30";
            var userAuthInfo = new UserAuthInfo(userId, "test@example.com", "Test User", true);
            var currentPassword = "WrongPassword123!";
            var newPassword = "NewPassword123!";

            _mockIdentityService.Setup(i => i.FindUserByIdAsync(userId)).ReturnsAsync(userAuthInfo);
            _mockIdentityService.Setup(i => i.ChangePasswordAsync(userId, currentPassword, newPassword))
                .ReturnsAsync(IdentityResultDto.Failure("Incorrect password"));

            var handler = new ChangePasswordCommandHandler(_mockIdentityService.Object, _mockRefreshTokenService.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(new ChangePasswordCommand(userId, currentPassword, newPassword), CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("*Failed to change password*");
        }
    }
}
