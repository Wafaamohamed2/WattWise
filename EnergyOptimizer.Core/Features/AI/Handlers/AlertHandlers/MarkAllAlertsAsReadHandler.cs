using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class MarkAllAlertsAsReadHandler : IRequestHandler<MarkAllAlertsAsReadCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public MarkAllAlertsAsReadHandler(IGenericRepository<Alert> alertRepo)
            => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(MarkAllAlertsAsReadCommand request, CancellationToken ct)
        {
            var spec = new AlertCountSpec(isRead: false);
            var unreadAlerts = await _alertRepo.ListAsync(spec);

            if (!unreadAlerts.Any())
                return new ApiResponse(200, "No unread alerts to mark");

            foreach (var alert in unreadAlerts)
            {
                alert.IsRead = true;
                _alertRepo.Update(alert);
            }

            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, "All alerts marked as read successfully");
        }
    }
}
