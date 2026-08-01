using EnergyOptimizer.Core.Behaviors;
using EnergyOptimizer.Core.Interfaces;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace EnergyOptimizer.Tests.Behaviors
{
    public class CachingBehaviorTests
    {
        private readonly Mock<IDistributedCache> _mockCache;
        private readonly Mock<ICurrentUserService> _mockUserService;
        private readonly Mock<ILogger<CachingBehavior<TestCacheableQuery, TestResponse>>> _mockLogger;
        private readonly Mock<ILogger<CacheInvalidationBehavior<TestInvalidatorCommand, TestResponse>>> _mockInvalidationLogger;

        public CachingBehaviorTests()
        {
            _mockCache = new Mock<IDistributedCache>();
            _mockUserService = new Mock<ICurrentUserService>();
            _mockUserService.Setup(u => u.UserId).Returns("user-123");

            _mockLogger = new Mock<ILogger<CachingBehavior<TestCacheableQuery, TestResponse>>>();
            _mockInvalidationLogger = new Mock<ILogger<CacheInvalidationBehavior<TestInvalidatorCommand, TestResponse>>>();
        }

        [Fact]
        public async Task Handle_CacheHit_ReturnsCachedDataWithoutCallingNextHandler()
        {
            // Arrange
            var query = new TestCacheableQuery("test-key-1");
            var expectedResponse = new TestResponse("Data from Cache");
            var serialized = JsonSerializer.Serialize(expectedResponse, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            _mockCache.Setup(c => c.GetAsync("test-key-1_user-123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(Encoding.UTF8.GetBytes(serialized));

            var behavior = new CachingBehavior<TestCacheableQuery, TestResponse>(_mockCache.Object, _mockLogger.Object, _mockUserService.Object);
            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult(new TestResponse("Data from DB"));
            };

            // Act
            var result = await behavior.Handle(query, next, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Data from Cache");
            nextCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CacheMiss_ExecutesNextHandlerAndSavesToCache()
        {
            // Arrange
            var query = new TestCacheableQuery("test-key-2");
            var dbResponse = new TestResponse("Data from Database");

            _mockCache.Setup(c => c.GetAsync("test-key-2_user-123", It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);

            var behavior = new CachingBehavior<TestCacheableQuery, TestResponse>(_mockCache.Object, _mockLogger.Object, _mockUserService.Object);
            var nextCalled = false;
            RequestHandlerDelegate<TestResponse> next = (ct) =>
            {
                nextCalled = true;
                return Task.FromResult(dbResponse);
            };

            // Act
            var result = await behavior.Handle(query, next, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Data from Database");
            nextCalled.Should().BeTrue();
            _mockCache.Verify(c => c.SetAsync("test-key-2_user-123", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_RedisException_FallsBackToDatabaseWithoutThrowing()
        {
            // Arrange
            var query = new TestCacheableQuery("test-key-3");
            var dbResponse = new TestResponse("Fallback DB Response");

            _mockCache.Setup(c => c.GetAsync("test-key-3_user-123", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Redis connection refused"));

            var behavior = new CachingBehavior<TestCacheableQuery, TestResponse>(_mockCache.Object, _mockLogger.Object, _mockUserService.Object);
            RequestHandlerDelegate<TestResponse> next = (ct) => Task.FromResult(dbResponse);

            // Act
            var result = await behavior.Handle(query, next, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Fallback DB Response");
        }

        [Fact]
        public async Task CacheInvalidationBehavior_RemovesTargetCacheKeysOnSuccess()
        {
            // Arrange
            var command = new TestInvalidatorCommand(new[] { "key-1", "key-2" });
            var response = new TestResponse("Success");
            var behavior = new CacheInvalidationBehavior<TestInvalidatorCommand, TestResponse>(_mockCache.Object, _mockInvalidationLogger.Object, _mockUserService.Object);

            RequestHandlerDelegate<TestResponse> next = (ct) => Task.FromResult(response);

            // Act
            var result = await behavior.Handle(command, next, CancellationToken.None);

            // Assert
            result.Message.Should().Be("Success");
            _mockCache.Verify(c => c.RemoveAsync("key-1_user-123", It.IsAny<CancellationToken>()), Times.Once);
            _mockCache.Verify(c => c.RemoveAsync("key-2_user-123", It.IsAny<CancellationToken>()), Times.Once);
        }

        public record TestCacheableQuery(string Key) : ICacheableRequest<TestResponse>
        {
            public string CacheKey => Key;
            public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
            public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromHours(1);
        }

        public record TestInvalidatorCommand(string[] Keys) : ICacheInvalidatorRequest<TestResponse>
        {
            public string[] CacheKeysToInvalidate => Keys;
        }

        public record TestResponse(string Message);
    }
}
