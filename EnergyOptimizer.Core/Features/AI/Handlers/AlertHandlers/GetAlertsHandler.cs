using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetAlertsHandler : IRequestHandler<GetAlertsQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public GetAlertsHandler(IGenericRepository<Alert> alertRepo)
        {
            _alertRepo = alertRepo;
        }

        public async Task<ApiResponse> Handle(GetAlertsQuery request, CancellationToken ct)
        {
            DateTime start = string.IsNullOrEmpty(request.StartDate)
                ? DateTime.UtcNow.AddDays(-7).Date
                : DateTime.Parse(request.StartDate);

            DateTime end = string.IsNullOrEmpty(request.EndDate)
                ? DateTime.UtcNow
                : DateTime.Parse(request.EndDate).AddDays(1).AddSeconds(-1);

            var spec = new AlertsWithFiltersSpec(request.IsRead, request.Severity, request.DeviceId, start, end);

            var totalAlerts = await _alertRepo.CountAsync(spec);
            var alerts = await _alertRepo.ListAsync(spec);

            var data = alerts.Select(a => new AlertDto
            {
                Id = a.Id,
                DeviceName = a.Device?.Name ?? "Unknown",
                ZoneName = a.Device?.Zone?.Name ?? "Unknown",
                AlertType = a.Type.ToString(),
                Message = a.Message,
                Severity = a.Severity,
                SeverityLabel = a.Severity == 1 ? "Info" : a.Severity == 2 ? "Warning" : "Critical",
                CreatedAt = a.CreatedAt,
                IsRead = a.IsRead
            }).ToList();

            return new ApiResponse(200, "Alerts retrieved successfully", new
            {
                pagination = new { totalAlerts, currentPage = request.Page },
                data
            });
        }
    }
}