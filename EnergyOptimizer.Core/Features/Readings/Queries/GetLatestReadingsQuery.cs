using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Readings.Queries
{
    public record GetLatestReadingsQuery(int Limit, string? StartDate = null, string? EndDate = null) 
        : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"LatestReadings_{Limit}_{StartDate ?? "none"}_{EndDate ?? "none"}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(5);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromSeconds(15);
    }
}
