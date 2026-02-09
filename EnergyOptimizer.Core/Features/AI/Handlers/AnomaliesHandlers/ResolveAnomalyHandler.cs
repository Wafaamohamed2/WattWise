using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers
{
    public class ResolveAnomalyHandler : IRequestHandler<ResolveAnomalyCommand, ApiResponse>
    {
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;

        public ResolveAnomalyHandler(IGenericRepository<DetectedAnomaly> anomalyRepo) => _anomalyRepo = anomalyRepo;

        public async Task<ApiResponse> Handle(ResolveAnomalyCommand request, CancellationToken ct)
        {
            var anomaly = await _anomalyRepo.GetByIdAsync(request.Id);
            if (anomaly == null) return new ApiResponse(404, "Anomaly not found");

            anomaly.IsResolved = true;
            anomaly.ResolvedAt = DateTime.UtcNow;
            anomaly.ResolutionNotes = request.ResolutionNotes;

            _anomalyRepo.Update(anomaly);
            await _anomalyRepo.SaveChangesAsync();

            return new ApiResponse(200, "Anomaly resolved successfully");
        }
    }
}
