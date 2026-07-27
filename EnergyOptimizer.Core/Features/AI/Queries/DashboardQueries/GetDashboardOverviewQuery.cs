using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetDashboardOverviewQuery : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => "Dashboard_Overview";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromHours(1);
    }
}
