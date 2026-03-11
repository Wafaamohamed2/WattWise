using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Specifications.AnalysisSpec;
using EnergyOptimizer.Core.Specifications.RecommendationSpec;
using EnergyOptimizer.Core.Specifications.AnomaliesSpec;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers
{
    public class GetAIStatisticsHandler : IRequestHandler<GetAIStatisticsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;

        public GetAIStatisticsHandler(
            IGenericRepository<EnergyAnalysis> analysisRepo,
            IGenericRepository<EnergyRecommendation> recommendationRepo,
            IGenericRepository<DetectedAnomaly> anomalyRepo)
        {
            _analysisRepo = analysisRepo;
            _recommendationRepo = recommendationRepo;
            _anomalyRepo = anomalyRepo;
        }

        public async Task<ApiResponse> Handle(GetAIStatisticsQuery request, CancellationToken ct)
        {
            try { 
            var today = DateTime.UtcNow;
            var thirtyDaysAgo = today.AddDays(-30);

            // Analyses
            var totalAnalyses = await _analysisRepo.CountAsync(new AnalysisHistoryCountSpec(null, null, null));
            var recentAnalyses = await _analysisRepo.CountAsync(new AnalysisHistoryCountSpec(null, thirtyDaysAgo, null));

            // Recommendations
            var allRecs = await _recommendationRepo.ListAsync(new RecommendationsFilterSpec(isImplemented: null));
            var activeRecs = allRecs.Count(r => !r.IsImplemented);
            var doneRecs = allRecs.Count(r => r.IsImplemented);
            var realizedSavings = allRecs.Where(r => r.IsImplemented).Sum(r => r.EstimatedSavingsKWh);
            var potentialSavings = allRecs.Where(r => !r.IsImplemented).Sum(r => r.EstimatedSavingsKWh);

            // Anomalies
            var allAnomalies = await _anomalyRepo.ListAsync(
                        new AnomaliesFilterSpec(null, null, null, 1, int.MaxValue));
            var totalAnomalies = allAnomalies.Count();
            var unresolved = allAnomalies.Count(a => !a.IsResolved);
            var critical = allAnomalies.Count(a => a.Severity == "Critical");
            var high = allAnomalies.Count(a => a.Severity == "High");
            var medium = allAnomalies.Count(a => a.Severity == "Medium");
            var low = allAnomalies.Count(a => a.Severity == "Low");
            var devicesAffected = allAnomalies.Select(a => a.DeviceId).Distinct().Count();

            var stats = new
            {
                analyses = new
                {
                    total = totalAnalyses,
                    last30Days = recentAnalyses
                },
                recommendations = new
                {
                    active = activeRecs,
                    implemented = doneRecs,
                    totalRealizedSavings = realizedSavings,
                    totalPotentialSavings = potentialSavings
                },
                anomalies = new
                {
                    total = totalAnomalies,
                    unresolved = unresolved,
                    criticalSeverityCount = critical,
                    highSeverityCount = high,
                    mediumSeverityCount = medium,
                    lowSeverityCount = low,
                    devicesAffected = devicesAffected
                }
            };

            return new ApiResponse(200, "Statistics retrieved successfully", stats);
        }
            catch (Exception ex)
            {
                return new ApiResponse(500, "Failed to retrieve statistics", new { error = ex.Message });
            }
        }
    }
}