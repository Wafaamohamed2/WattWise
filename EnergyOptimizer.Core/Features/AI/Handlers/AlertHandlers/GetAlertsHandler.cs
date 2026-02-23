using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;
using AutoMapper;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetAlertsHandler : IRequestHandler<GetAlertsQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IMapper _mapper;

        public GetAlertsHandler(IGenericRepository<Alert> alertRepo , IMapper mapper)
        {
            _alertRepo = alertRepo;
            _mapper = mapper;
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

            var data = _mapper.Map<List<AlertDto>>(alerts);
            return new ApiResponse(200, "Alerts retrieved successfully", new { data });
        }
    }
}