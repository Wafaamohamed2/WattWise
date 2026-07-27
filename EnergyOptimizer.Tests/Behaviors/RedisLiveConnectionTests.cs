using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;

namespace EnergyOptimizer.Tests.Behaviors
{
    public class RedisLiveConnectionTests
    {
        [Fact]
        public async Task LiveRedisCloud_SetAndGet_ShouldSucceed()
        {
            // Arrange - Load RedisConnection dynamically from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile(@"..\..\..\..\EnergyOptimizer.API\appsettings.json", optional: true)
                .Build();

            var connectionString = configuration.GetConnectionString("RedisConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // Skip test if no active connection string is configured
                return;
            }

            var options = new RedisCacheOptions
            {
                Configuration = connectionString,
                InstanceName = "WattWise_LiveTest_"
            };

            var cache = new RedisCache(options);
            var testKey = "PingKey_" + Guid.NewGuid();
            var testValue = "Redis Cloud Connected Successfully!";

            // Act
            await cache.SetStringAsync(testKey, testValue);
            var retrievedValue = await cache.GetStringAsync(testKey);

            // Assert
            retrievedValue.Should().Be(testValue);
        }
    }
}
