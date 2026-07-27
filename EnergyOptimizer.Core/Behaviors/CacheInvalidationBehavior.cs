using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Core.Behaviors
{
    public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheInvalidatorRequest<TResponse>
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<CacheInvalidationBehavior<TRequest, TResponse>> _logger;

        public CacheInvalidationBehavior(IDistributedCache cache, ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            if (request.CacheKeysToInvalidate != null && request.CacheKeysToInvalidate.Length > 0)
            {
                foreach (var key in request.CacheKeysToInvalidate)
                {
                    if (string.IsNullOrWhiteSpace(key)) continue;

                    try
                    {
                        await _cache.RemoveAsync(key, cancellationToken);
                        _logger.LogInformation("Invalidated Cache Key -> '{CacheKey}'", key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to invalidate Cache Key -> '{CacheKey}'", key);
                    }
                }
            }

            return response;
        }
    }
}
