using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.Alerts.Queries
{
    public record GetAlertStatisticsQuery(string? StartDate, int Days) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"AlertStats_{Days}_{StartDate ?? "none"}";
        public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(10);
        public TimeSpan? AbsoluteExpirationRelativeToNow => TimeSpan.FromHours(1);
    }
}
