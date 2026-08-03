using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.Alerts.Queries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Features.Alerts.Handlers
{
    public class GetAlertStatisticsHandler : IRequestHandler<GetAlertStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly ICurrentUserService _currentUser;

        public GetAlertStatisticsHandler(IGenericRepository<Alert> alertRepo, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAlertStatisticsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            DateTime start = string.IsNullOrEmpty(request.StartDate)
                ? DateTime.UtcNow.AddDays(-request.Days).Date
                : DateTime.Parse(request.StartDate);

            var totalCount = await _alertRepo.CountAsync(new AlertCountSpec(userId, startDate: start));
            var unreadCount = await _alertRepo.CountAsync(new AlertCountSpec(userId, isRead: false, startDate: start));
            var criticalCount = await _alertRepo.CountAsync(new AlertCountSpec(userId, severity: AlertSeverity.Critical, startDate: start));
            var warningCount = await _alertRepo.CountAsync(new AlertCountSpec(userId, severity: AlertSeverity.Warning, startDate: start));
            var infoCount = await _alertRepo.CountAsync(new AlertCountSpec(userId, severity: AlertSeverity.Info, startDate: start));

            var statistics = new AlertStatistics
            {
                TotalAlerts = totalCount,
                UnreadAlerts = unreadCount,
                CriticalAlerts = criticalCount,
                WarningAlerts = warningCount,
                InfoAlerts = infoCount
            };

            return new ApiResponse(200, "Statistics retrieved successfully", statistics);
        }
    }
}