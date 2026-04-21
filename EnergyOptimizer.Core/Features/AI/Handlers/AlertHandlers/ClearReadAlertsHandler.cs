using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;


namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class ClearReadAlertsHandler : IRequestHandler<ClearReadAlertsCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public ClearReadAlertsHandler(IGenericRepository<Alert> alertRepo)
            => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(ClearReadAlertsCommand request, CancellationToken ct)
        {
            var spec = new AlertCountSpec(isRead: true);
            var readAlerts = await _alertRepo.ListAsync(spec);

            if (!readAlerts.Any())
                return new ApiResponse(200, "No read alerts to clear");

            _alertRepo.DeleteRange(readAlerts);
            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, $"{readAlerts.Count} read alerts cleared");
        }
    }
}
