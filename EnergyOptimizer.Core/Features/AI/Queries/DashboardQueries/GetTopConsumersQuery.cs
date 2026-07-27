using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetTopConsumersQuery(int Count, string StartDate) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"TopConsumers_{Count}_{StartDate}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromHours(1);
    }
}
