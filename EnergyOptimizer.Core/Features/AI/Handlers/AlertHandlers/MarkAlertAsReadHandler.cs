using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class MarkAlertAsReadHandler : IRequestHandler<MarkAlertAsReadCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public MarkAlertAsReadHandler(IGenericRepository<Alert> alertRepo)
            => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(MarkAlertAsReadCommand request, CancellationToken ct)
        {
            var alert = await _alertRepo.GetByIdAsync(request.Id);

            if (alert == null)
                throw new NotFoundException($"Alert with ID {request.Id} not found");

            alert.IsRead = true;
            _alertRepo.Update(alert);
            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, "Alert marked as read");
        }
    }
}
