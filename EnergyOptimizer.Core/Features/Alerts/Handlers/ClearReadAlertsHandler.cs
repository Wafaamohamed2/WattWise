using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.Alerts.Commands;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Core.Features.Alerts.Handlers
{
    public class ClearReadAlertsHandler : IRequestHandler<ClearReadAlertsCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly ICurrentUserService _currentUser;

        public ClearReadAlertsHandler(IGenericRepository<Alert> alertRepo, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(ClearReadAlertsCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            var readAlerts = await _alertRepo.GetQueryable()
                .Where(a => a.IsRead &&
                            a.Device != null && a.Device.Zone != null && a.Device.Zone.Building != null &&
                            a.Device.Zone.Building.UserId == userId &&
                            (!request.BuildingId.HasValue || a.Device.Zone.BuildingId == request.BuildingId.Value))
                .ToListAsync(ct);

            if (!readAlerts.Any())
                return new ApiResponse(200, "No read alerts to clear");

            _alertRepo.DeleteRange(readAlerts);
            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, $"{readAlerts.Count} read alerts cleared");
        }
    }
}
