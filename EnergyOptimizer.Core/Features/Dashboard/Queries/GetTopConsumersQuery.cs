using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetTopConsumersQuery(int Count, string? StartDate = null) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"TopConsumers_{Count}_{StartDate ?? "all"}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromHours(1);
    }
}
