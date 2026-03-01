using AutoMapper;
using EnergyOptimizer.API.Controllers;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static EnergyOptimizer.API.DTOs.AuthDto;
using EnergyOptimizer.Service.Services;

namespace EnergyOptimizer.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJwtTokenService> _mockTokenService; 
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockMapper = new Mock<IMapper>();
            _mockTokenService = new Mock<IJwtTokenService>(); 

            _controller = new AccountController(_mockUserManager.Object, _mockMapper.Object, _mockTokenService.Object);
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOk()
        {
            // Arrange 
            var registerDto = new RegisterDto("test@example.com", "Password123!", "Wafaa Mohamed");
            var user = new ApplicationUser { Email = registerDto.Email };

            _mockMapper.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(user);
            _mockUserManager.Setup(u => u.CreateAsync(user, registerDto.Password))
                            .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            _mockUserManager.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto("existing@example.com", "Password123!", "Any Name");
            var user = new ApplicationUser { Email = registerDto.Email };

            _mockMapper.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(user);
            _mockUserManager.Setup(u => u.CreateAsync(user, registerDto.Password))
                            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Email already exists" }));

            // Act
            Func<Task> act = async () => await _controller.Register(registerDto);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            // Arrange 
            var loginDto = new LoginDto("test@example.com", "Password123!");
            var user = new ApplicationUser { Id = "1", Email = loginDto.Email, FullName = "Ali Mohamed", UserName = "test@example.com" };
            var fakeToken = "fake-jwt-token";

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);

            _mockTokenService.Setup(t => t.GenerateToken(user)).Returns(fakeToken);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            _mockTokenService.Verify(t => t.GenerateToken(user), Times.Once);

            var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            responseJson.Should().Contain(fakeToken);
        }

        [Fact]
        public async Task Login_InvalidPassword_ThrowsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto("test@example.com", "WrongPassword");
            var user = new ApplicationUser { Email = loginDto.Email };

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _controller.Login(loginDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedException>();

            _mockTokenService.Verify(t => t.GenerateToken(It.IsAny<ApplicationUser>()), Times.Never);
        }
    }
}