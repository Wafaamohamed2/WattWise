using MediatR;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface ICacheableRequest<TResponse> : IRequest<TResponse>
    {
        string CacheKey { get; }
        TimeSpan? SlidingExpiration { get; }
        TimeSpan? AbsoluteExpirationRelativeToNow { get; }
    }
}
