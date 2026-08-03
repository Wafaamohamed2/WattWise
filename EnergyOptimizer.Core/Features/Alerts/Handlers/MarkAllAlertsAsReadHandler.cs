using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Alerts.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Core.Features.Alerts.Handlers
{
    public class MarkAllAlertsAsReadHandler : IRequestHandler<MarkAllAlertsAsReadCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly ICurrentUserService _currentUser;

        public MarkAllAlertsAsReadHandler(IGenericRepository<Alert> alertRepo, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(MarkAllAlertsAsReadCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            var updatedRows = await _alertRepo.GetQueryable()
                .Where(a => !a.IsRead &&
                            a.Device != null && a.Device.Zone != null && a.Device.Zone.Building != null &&
                            a.Device.Zone.Building.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsRead, true), ct);

            if (updatedRows == 0)
                return new ApiResponse(200, "No unread alerts to mark");

            return new ApiResponse(200, $"{updatedRows} alerts marked as read successfully");
        }
    }
}
