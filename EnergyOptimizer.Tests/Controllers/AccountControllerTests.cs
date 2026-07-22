using EnergyOptimizer.API.Controllers;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using EnergyOptimizer.Core.Features.Auth.Handlers;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static EnergyOptimizer.Core.DTOs.AuthDto;

namespace EnergyOptimizer.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _mockConfig;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            _mockEnv.Setup(e => e.EnvironmentName).Returns("Development");
            _mockConfig = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();

            _controller = new AccountController(_mockMediator.Object, _mockEnv.Object, _mockConfig.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Register_ValidData_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDto("Wafaa Mohamed", "test@example.com", "Password123!");
            var expectedResponse = new ApiResponse(200, "User registered successfully!");

            _mockMediator.Setup(m => m.Send(It.Is<RegisterCommand>(c => c.Dto == registerDto), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            okResult.Value.Should().Be(expectedResponse);
        }

        [Fact]
        public async Task Login_ValidCredentials_SetsAccessTokenAndRefreshTokenCookies()
        {
            // Arrange
            var loginDto = new LoginDto("test@example.com", "Password123!");
            var userDto = new LoginUserDto("1", "Ali Mohamed", "test@example.com");
            var loginDetails = new LoginResultDetails("fake-jwt-token", "fake-refresh-token", userDto);
            var expectedResponse = new ApiResponse(200, "Login successful", loginDetails);

            _mockMediator.Setup(m => m.Send(It.Is<LoginCommand>(c => c.Dto == loginDto), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;

            // Cookies set on response
            var setCookieHeader = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("access_token");
            setCookieHeader.Should().Contain("refresh_token");
            setCookieHeader.Should().Contain("httponly");

            // Tokens are NOT in the response body
            var responseJson = System.Text.Json.JsonSerializer.Serialize(okResult.Value);
            responseJson.Should().NotContain("fake-jwt-token");
            responseJson.Should().NotContain("fake-refresh-token");
            responseJson.Should().Contain("User");
        }

        [Fact]
        public async Task RefreshToken_ValidCookie_RotatesTokensAndSetsNewCookies()
        {
            // Arrange
            _controller.HttpContext.Request.Headers["Cookie"] = "refresh_token=valid-refresh-token";
            var refreshDetails = new RefreshTokenResultDetails("new-jwt-token", "new-refresh-token");
            var expectedResponse = new ApiResponse(200, "Token refreshed successfully", refreshDetails);

            _mockMediator.Setup(m => m.Send(It.Is<RefreshTokenCommand>(c => c.RefreshToken == "valid-refresh-token"), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.RefreshToken(null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var setCookieHeader = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("access_token");
            setCookieHeader.Should().Contain("refresh_token");
        }

        [Fact]
        public async Task Logout_RevokesTokenAndClearsCookies()
        {
            // Arrange
            _controller.HttpContext.Request.Headers["Cookie"] = "refresh_token=valid-refresh-token";
            var revokeResponse = new ApiResponse(200, "Token revoked successfully");

            _mockMediator.Setup(m => m.Send(It.Is<RevokeTokenCommand>(c => c.RefreshToken == "valid-refresh-token"), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(revokeResponse);

            // Act
            var result = await _controller.Logout(null);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var setCookieHeader = _controller.HttpContext.Response.Headers["Set-Cookie"].ToString();
            setCookieHeader.Should().Contain("access_token");
            setCookieHeader.Should().Contain("refresh_token");
            setCookieHeader.Should().Contain("expires=");
        }
    }
}
