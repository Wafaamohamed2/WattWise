using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;

namespace EnergyOptimizer.Core.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheableRequest<TResponse>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };

        public CachingBehavior(IDistributedCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var cacheKey = request.CacheKey;

            if (string.IsNullOrWhiteSpace(cacheKey))
            {
                return await next();
            }

            // Fast Path: Try reading from Distributed Cache without acquiring lock
            var cachedResponse = await TryGetFromCacheAsync(cacheKey, cancellationToken);
            if (cachedResponse != null)
            {
                _logger.LogInformation("Fetched from Cache (Fast Path) -> '{CacheKey}'", cacheKey);
                return cachedResponse;
            }

            // Cache Stampede Protection: Acquire lock per specific cacheKey
            var keyLock = _locks.GetOrAdd(cacheKey, _ => new SemaphoreSlim(1, 1));
            await keyLock.WaitAsync(cancellationToken);

            try
            {
                // Double-Checked Locking: Re-check cache after acquiring lock
                cachedResponse = await TryGetFromCacheAsync(cacheKey, cancellationToken);
                if (cachedResponse != null)
                {
                    _logger.LogInformation("Fetched from Cache (Double-Check Path) -> '{CacheKey}'", cacheKey);
                    return cachedResponse;
                }

                // Cache Miss -> Fetch from Database / Handler
                _logger.LogInformation("Cache Miss -> '{CacheKey}'. Fetching from Database...", cacheKey);
                var response = await next();

                // Save to Cache
                if (response != null)
                {
                    await TrySaveToCacheAsync(cacheKey, response, request, cancellationToken);
                }

                return response;
            }
            finally
            {
                keyLock.Release();
            }
        }

        private async Task<TResponse?> TryGetFromCacheAsync(string cacheKey, CancellationToken cancellationToken)
        {
            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    return JsonSerializer.Deserialize<TResponse>(cachedData, SerializerOptions);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis Cache Error for key '{CacheKey}'. Falling back to Database.", cacheKey);
            }

            return default;
        }

        private async Task TrySaveToCacheAsync(string cacheKey, TResponse response, TRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    SlidingExpiration = request.SlidingExpiration ?? TimeSpan.FromMinutes(30),
                    AbsoluteExpirationRelativeToNow = request.AbsoluteExpirationRelativeToNow
                };

                var serializedData = JsonSerializer.Serialize(response, SerializerOptions);
                await _cache.SetStringAsync(cacheKey, serializedData, options, cancellationToken);
                _logger.LogInformation("Saved to Cache -> '{CacheKey}'", cacheKey);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save response to Redis Cache for key '{CacheKey}'.", cacheKey);
            }
        }
    }
}
