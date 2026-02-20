using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;


namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class MarkAllAlertsAsReadHandler : IRequest<ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public MarkAllAlertsAsReadHandler(IGenericRepository<Alert> alertRepo) => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(MarkAllAlertsAsReadCommand request, CancellationToken ct)
        {
            var spec = new AlertCountSpec(isRead: false);
            var unreadAlerts = await _alertRepo.ListAsync(spec);

            foreach (var alert in unreadAlerts)
            {
                alert.IsRead = true;
                _alertRepo.Update(alert);
            }

            await _alertRepo.SaveChangesAsync();
            return new ApiResponse(200, $"{unreadAlerts.Count} alerts marked as read");
        }
    }
}
