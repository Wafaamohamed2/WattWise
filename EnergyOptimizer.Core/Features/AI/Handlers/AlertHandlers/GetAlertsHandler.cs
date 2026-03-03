using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using EnergyOptimizer.Core.Features.AI.Commands;
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

            var countSpec = new AlertsWithFiltersSpec(request.IsRead, request.Severity, request.DeviceId, start, end);
            var total = await _alertRepo.CountAsync(countSpec);

            var pagedSpec = new AlertsWithFiltersSpec(
               request.IsRead, request.Severity, request.DeviceId,
               start, end,
               page: request.Page, pageSize: request.PageSize);

            var alerts = await _alertRepo.ListAsync(pagedSpec);
            var data = _mapper.Map<List<AlertDto>>(alerts);

            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            return new ApiResponse(200, "Alerts retrieved successfully", new
            {
                page = request.Page,
                pageSize = request.PageSize,
                total,
                totalPages,
                data
            });
        }
    }
}