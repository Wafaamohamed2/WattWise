using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public GetAlertByIdHandler(IGenericRepository<Alert> alertRepo) => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(GetAlertByIdQuery request, CancellationToken ct)
        {
            var spec = new AlertsWithFiltersSpec(null, null, null, DateTime.MinValue, DateTime.MaxValue);  

            var alert = (await _alertRepo.ListAsync(spec)).FirstOrDefault(a => a.Id == request.Id);

            if (alert == null) throw new NotFoundException($"Alert with ID {request.Id} not found");

            var dto = new AlertDto
            {
                Id = alert.Id,
                DeviceName = alert.Device?.Name ?? "Unknown",
                ZoneName = alert.Device?.Zone?.Name ?? "Unknown",
                AlertType = alert.Type.ToString(),
                Message = alert.Message,
                Severity = alert.Severity,
                SeverityLabel = alert.Severity == 1 ? "Info" : alert.Severity == 2 ? "Warning" : "Critical",
                CreatedAt = alert.CreatedAt,
                IsRead = alert.IsRead
            };

            return new ApiResponse(200, "Alert retrieved successfully", dto);
        }
    }
}
