using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetConsumptionTrendQuery(int Hours, int? BuildingId = null) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"ConsumptionTrend_{Hours}_{BuildingId}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(3);
    }
}
