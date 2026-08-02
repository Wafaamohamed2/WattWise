using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
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

            var deletedRows = await _alertRepo.GetQueryable()
                .Where(a => a.IsRead &&
                            a.Device != null && a.Device.Zone != null && a.Device.Zone.Building != null &&
                            a.Device.Zone.Building.UserId == userId)
                .ExecuteDeleteAsync(ct);

            if (deletedRows == 0)
                return new ApiResponse(200, "No read alerts to clear");

            return new ApiResponse(200, $"{deletedRows} read alerts cleared");
        }
    }
}
