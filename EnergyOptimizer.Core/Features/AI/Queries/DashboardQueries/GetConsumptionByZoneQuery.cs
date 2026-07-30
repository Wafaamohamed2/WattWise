using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetConsumptionByZoneQuery(string? StartDate, string? EndDate) : ICacheableRequest<ApiResponse>
    {
        public string CacheKey => $"ConsumptionByZone_{StartDate ?? "none"}_{EndDate ?? "none"}";
        
        private bool IsHistorical => DateTime.TryParse(EndDate, out var end) && end.Date < DateTime.UtcNow.Date;
        public TimeSpan? SlidingExpiration => IsHistorical ? TimeSpan.FromHours(12) : TimeSpan.FromMinutes(2);
        public TimeSpan? AbsoluteExpirationRelativeToNow => IsHistorical ? TimeSpan.FromDays(7) : TimeSpan.FromMinutes(5);
    }
}
