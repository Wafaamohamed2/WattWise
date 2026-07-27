using MediatR;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface ICacheInvalidatorRequest<TResponse> : IRequest<TResponse>
    {
        string[] CacheKeysToInvalidate { get; }
    }
}
