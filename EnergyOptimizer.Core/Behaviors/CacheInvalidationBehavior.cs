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
        private readonly ICurrentUserService? _currentUserService;

        public CacheInvalidationBehavior(
            IDistributedCache cache, 
            ILogger<CacheInvalidationBehavior<TRequest, TResponse>> logger,
            ICurrentUserService? currentUserService = null)
        {
            _cache = cache;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var response = await next();

            if (request.CacheKeysToInvalidate != null && request.CacheKeysToInvalidate.Length > 0)
            {
                var userId = _currentUserService?.UserId ?? "anonymous";

                foreach (var rawKey in request.CacheKeysToInvalidate)
                {
                    if (string.IsNullOrWhiteSpace(rawKey)) continue;

                    var keyWithUser = $"{rawKey}_{userId}";

                    try
                    {
                        await _cache.RemoveAsync(keyWithUser, cancellationToken);
                        _logger.LogInformation("Invalidated Cache Key -> '{CacheKey}'", keyWithUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to invalidate Cache Key -> '{CacheKey}'", keyWithUser);
                    }
                }
            }

            return response;
        }
    }
}
