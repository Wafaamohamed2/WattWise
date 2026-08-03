using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Alerts.Commands;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;

namespace EnergyOptimizer.Core.Features.Alerts.Handlers
{
    public class DeleteAlertHandler : IRequestHandler<DeleteAlertCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly ICurrentUserService _currentUser;

        public DeleteAlertHandler(IGenericRepository<Alert> alertRepo, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(DeleteAlertCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new AlertOwnedByUserSpec(request.Id, userId);
            var alert = await _alertRepo.GetEntityWithSpec(spec);

            if (alert == null)
                throw new NotFoundException($"Alert with ID {request.Id} not found");

            _alertRepo.Delete(alert);
            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, "Alert deleted successfully");
        }
    }
}
