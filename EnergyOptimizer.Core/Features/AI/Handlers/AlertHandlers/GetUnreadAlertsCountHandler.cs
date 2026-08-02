using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetUnreadAlertsCountHandler : IRequestHandler<GetUnreadAlertsCountQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly ICurrentUserService _currentUser;

        public GetUnreadAlertsCountHandler(IGenericRepository<Alert> alertRepo, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetUnreadAlertsCountQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new AlertCountSpec(userId, isRead: false);
            var count = await _alertRepo.CountAsync(spec);

            return new ApiResponse(200, "Unread alerts count retrieved", new { count });
        }
    }
}
