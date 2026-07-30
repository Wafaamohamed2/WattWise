using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetDashboardOverviewQuery : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => "Dashboard_Overview";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(1);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromMinutes(3);
    }
}
