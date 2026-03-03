using EnergyOptimizer.API.Middleware;
using EnergyOptimizer.Core.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;

namespace Energy_Optimizer_Test
{
    public class ExceptionMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionMiddleware>> _mockLogger;
        private readonly Mock<IHostEnvironment> _mockEnv;
        private readonly DefaultHttpContext _context;

        public ExceptionMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<ExceptionMiddleware>>();
            _mockEnv = new Mock<IHostEnvironment>();
            _context = new DefaultHttpContext();
            _context.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
        {
            //Arrange
            var nextCalled = false;
            RequestDelegate next = (ctx) =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            //Act
            await middleware.InvokeAsync(_context);

            //Assert
            nextCalled.Should().BeTrue();
            _context.Response.StatusCode.Should().Be(200); 
        }

        [Fact]
        public async Task InvokeAsync_WhenBaseExceptionOccurs_ReturnsSpecificStatusCode()
        {
            // Arrange
            var exceptionMessage = "Resource not found";
            var statusCode = 404;
            RequestDelegate next = (ctx)=> throw new MockBaseException(exceptionMessage, statusCode);

            _mockEnv.Setup(m => m.EnvironmentName).Returns(Environments.Production);
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(statusCode);
            _context.Response.ContentType.Should().Be("application/json");

            var responseBody = await ReadResponseBody();
            responseBody.StatusCode.Should().Be(statusCode);
            responseBody.Message.Should().Be(exceptionMessage);
        }

        [Fact]
        public async Task InvokeAsync_WhenUnknownExceptionOccurs_ReturnsInternalServerError()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new Exception("Unexpected crash");
            _mockEnv.Setup(m => m.EnvironmentName).Returns(Environments.Production);
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.StatusCode.Should().Be(500);

            var responseBody = await ReadResponseBody();
            responseBody.Message.Should().Be("Internal Server Error");
        }

        [Fact]
        public async Task InvokeAsync_InDevelopmentMode_ShouldIncludeStackTrace()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new Exception("Debug this error");
            _mockEnv.Setup(m => m.EnvironmentName).Returns(Environments.Development);
            var middleware = new ExceptionMiddleware(next, _mockLogger.Object, _mockEnv.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            var responseBody = await ReadResponseBody();
            responseBody.Details.Should().NotBeNull();
        }

        private async Task<ExceptionMiddleware.ApiResponse> ReadResponseBody()
        {
            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_context.Response.Body);
            var json = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<ExceptionMiddleware.ApiResponse>(json,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!;
        }
        private class MockBaseException : BaseException
        {
            public MockBaseException(string message, int statusCode) : base(message, statusCode) { }
        }
    }
}
