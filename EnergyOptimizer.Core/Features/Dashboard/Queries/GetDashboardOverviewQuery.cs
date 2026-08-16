using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Dashboard.Queries
{
    public record GetDashboardOverviewQuery(int? BuildingId = null) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"Dashboard_Overview_{BuildingId}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(3);
    }
}
