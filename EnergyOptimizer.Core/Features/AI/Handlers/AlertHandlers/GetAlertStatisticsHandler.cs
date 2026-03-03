using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetAlertStatisticsHandler : IRequestHandler<GetAlertStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public GetAlertStatisticsHandler(IGenericRepository<Alert> alertRepo) => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(GetAlertStatisticsQuery request, CancellationToken ct)
        {
            DateTime start = string.IsNullOrEmpty(request.StartDate)
                ? DateTime.UtcNow.AddDays(-request.Days).Date
                : DateTime.Parse(request.StartDate);

            var spec = new AlertsByDateSpec(start);
            var alerts = await _alertRepo.ListAsync(spec);

            var statistics = new AlertStatistics
            {
                TotalAlerts = alerts.Count,
                UnreadAlerts = alerts.Count(a => !a.IsRead),
                CriticalAlerts = alerts.Count(a => a.Severity == 3),
                WarningAlerts = alerts.Count(a => a.Severity == 2),
                InfoAlerts = alerts.Count(a => a.Severity == 1)
            };

            return new ApiResponse(200, "Statistics retrieved successfully", statistics);
        }
    }
}