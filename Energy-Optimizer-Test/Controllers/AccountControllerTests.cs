using AutoMapper;
using EnergyOptimizer.API.Controllers;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Service.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static EnergyOptimizer.API.DTOs.AuthDto;

namespace EnergyOptimizer.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IJwtTokenService> _mockTokenService;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _mockMapper = new Mock<IMapper>();
            _mockTokenService = new Mock<IJwtTokenService>();

            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");

            _controller = new AccountController(
                _mockUserManager.Object,
                _mockMapper.Object,
                _mockTokenService.Object,
                _mockEnv.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
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
            _mockUserManager.Verify(
                u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()),
                Times.Once);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ThrowsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto("existing@example.com", "Password123!", "Any Name");
            var user = new ApplicationUser { Email = registerDto.Email };

            _mockMapper.Setup(m => m.Map<ApplicationUser>(registerDto)).Returns(user);
            _mockUserManager.Setup(u => u.CreateAsync(user, registerDto.Password))
                            .ReturnsAsync(IdentityResult.Failed(
                                new IdentityError { Description = "Email already exists" }));

            // Act
            Func<Task> act = async () => await _controller.Register(registerDto);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task Login_ValidCredentials_SetsHttpOnlyCookieAndReturnsUser()
        {
            // Arrange
            var loginDto = new LoginDto("test@example.com", "Password123!");
            var user = new ApplicationUser
            {
                Id = "1",
                Email = loginDto.Email,
                FullName = "Ali Mohamed",
                UserName = "test@example.com"
            };

            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email)).ReturnsAsync(user);
            _mockUserManager.Setup(u => u.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
            _mockTokenService.Setup(t => t.GenerateToken(user)).Returns("fake-jwt-token");

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Token service was called
            _mockTokenService.Verify(t => t.GenerateToken(user), Times.Once);

            // Cookie was set on the response
            var setCookieHeader = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("access_token");
            setCookieHeader.Should().Contain("httponly", Exactly.Once());

            // Token is NOT in the response body (removed to prevent XSS via localStorage)
            var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            responseJson.Should().NotContain("fake-jwt-token");
            responseJson.Should().Contain("User");
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
            _mockTokenService.Verify(
                t => t.GenerateToken(It.IsAny<ApplicationUser>()),
                Times.Never);
        }

        [Fact]
        public async Task Login_UserNotFound_ThrowsBadRequest()
        {
            // Arrange
            var loginDto = new LoginDto("notfound@example.com", "Password123!");
            _mockUserManager.Setup(u => u.FindByEmailAsync(loginDto.Email))
                            .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _controller.Login(loginDto);

            // Assert
            await act.Should().ThrowAsync<BadRequestException>();
        }
    }
}