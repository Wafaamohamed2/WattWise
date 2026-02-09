using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers
{
    public class GetAIStatisticsHandler : IRequestHandler<GetAIStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recRepo;
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;
        private readonly IGenericRepository<Device> _deviceRepo;

        public GetAIStatisticsHandler(IGenericRepository<EnergyAnalysis> analysisRepo, IGenericRepository<EnergyRecommendation> recRepo,
            IGenericRepository<DetectedAnomaly> anomalyRepo, IGenericRepository<Device> deviceRepo)
        {
            _analysisRepo = analysisRepo; _recRepo = recRepo; _anomalyRepo = anomalyRepo; _deviceRepo = deviceRepo;
        }

        public async Task<ApiResponse> Handle(GetAIStatisticsQuery request, CancellationToken ct)
        {
            var analyses = await _analysisRepo.ListAllAsync();
            var recommendations = await _recRepo.ListAllAsync();
            var anomalies = await _anomalyRepo.ListAllAsync();

            var stats = new
            {
                Analyses = new { Total = analyses.Count(), Last30Days = analyses.Count(a => a.AnalysisDate >= DateTime.UtcNow.AddDays(-30)) },
                Recommendations = new
                {
                    Total = recommendations.Count(),
                    Active = recommendations.Count(r => !r.IsImplemented && (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)),
                    TotalPotentialSavings = Math.Round(recommendations.Where(r => !r.IsImplemented).Sum(r => (double)r.EstimatedSavingsKWh), 2)
                },
                Anomalies = new
                {
                    Total = anomalies.Count(),
                    Unresolved = anomalies.Count(a => !a.IsResolved),
                    bySeverity = anomalies.GroupBy(a => a.Severity).Select(g => new { severity = g.Key, count = g.Count() })
                }
            };

            return new ApiResponse(200, "AI Statistics retrieved successfully", stats);
        }
    }
}
