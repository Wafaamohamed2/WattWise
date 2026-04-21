using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands.AlertsCommans;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Interfaces;
using MediatR;


namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class DeleteAlertHandler : IRequestHandler<DeleteAlertCommand, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;

        public DeleteAlertHandler(IGenericRepository<Alert> alertRepo)
            => _alertRepo = alertRepo;

        public async Task<ApiResponse> Handle(DeleteAlertCommand request, CancellationToken ct)
        {
            var alert = await _alertRepo.GetByIdAsync(request.Id);

            if (alert == null)
                throw new NotFoundException($"Alert with ID {request.Id} not found");

            _alertRepo.Delete(alert);
            await _alertRepo.SaveChangesAsync();

            return new ApiResponse(200, "Alert deleted successfully");
        }
    }
}
