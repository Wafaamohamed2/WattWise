using AutoMapper;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Features.Auth.Handlers;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using static EnergyOptimizer.Core.DTOs.AuthDto;

namespace EnergyOptimizer.Tests.Handlers.Auth
{
    public class AuthHandlersTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJwtTokenService> _mockTokenService;

        public AuthHandlersTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockMapper = new Mock<IMapper>();
            _mockTokenService = new Mock<IJwtTokenService>();
        }

        [Fact]
        public async Task RegisterHandler_ValidData_CreatesUser()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);
            var user = new ApplicationUser { Email = dto.Email };

            _mockMapper.Setup(m => m.Map<ApplicationUser>(dto)).Returns(user);
            _mockUserManager.Setup(u => u.CreateAsync(user, dto.Password)).ReturnsAsync(IdentityResult.Success);

            var handler = new RegisterCommandHandler(_mockUserManager.Object, _mockMapper.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            result.Message.Should().Be("User registered successfully!");
        }

        [Fact]
        public async Task RegisterHandler_FailedCreation_ThrowsBadRequest()
        {
            // Arrange
            var dto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var command = new RegisterCommand(dto);
            var user = new ApplicationUser { Email = dto.Email };

            _mockMapper.Setup(m => m.Map<ApplicationUser>(dto)).Returns(user);
            _mockUserManager.Setup(u => u.CreateAsync(user, dto.Password))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email taken" }));

            var handler = new RegisterCommandHandler(_mockUserManager.Object, _mockMapper.Object);

            // Act
            Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>().WithMessage("Email taken");
        }

        [Fact]
        public async Task LoginHandler_ValidCredentials_ReturnsTokenAndUser()
        {
            // Arrange
            var dto = new LoginDto("test@example.com", "Password123!");
            var command = new LoginCommand(dto);
            var user = new ApplicationUser { Id = "1", Email = dto.Email, FullName = "Ali Mohamed" };

            _mockUserManager.Setup(u => u.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
            _mockTokenService.Setup(t => t.GenerateToken(user)).Returns("fake-jwt-token");

            var handler = new LoginCommandHandler(_mockUserManager.Object, _mockTokenService.Object);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.StatusCode.Should().Be(200);
            var details = result.Details.Should().BeOfType<LoginResultDetails>().Subject;
            details.Token.Should().Be("fake-jwt-token");
            details.User.Id.Should().Be("1");
            details.User.FullName.Should().Be("Ali Mohamed");
        }
    }
}
