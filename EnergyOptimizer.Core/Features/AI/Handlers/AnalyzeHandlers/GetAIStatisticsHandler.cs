using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

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
            var today = DateTime.UtcNow;
            var thirtyDaysAgo = today.AddDays(-30);
            var sevenDaysAgo = today.AddDays(-7);

            // Analyses
            var allAnalyses = await _analysisRepo.ListAllAsync();
            int totalCount = allAnalyses.Count;
            int count30 = allAnalyses.Count(a => a.AnalysisDate >= thirtyDaysAgo);

            // Recommendations
            var allRecs = await _recommendationRepo.ListAllAsync();
            int activeRecs = allRecs.Count(r => !r.IsImplemented);
            int doneRecs = allRecs.Count(r => r.IsImplemented);
            double realizedSavings = allRecs.Where(r => r.IsImplemented).Sum(r => r.EstimatedSavingsKWh);
            double potentialSavings = allRecs.Where(r => !r.IsImplemented).Sum(r => r.EstimatedSavingsKWh);

            // Anomalies
            var allAnomalies = await _anomalyRepo.ListAllAsync();
            int pendingAnoms = allAnomalies.Count(a => !a.IsResolved);
            int recentAnoms = allAnomalies.Count(a => a.DetectedAt >= sevenDaysAgo);

            var stats = new
            {
                analyses = new
                {
                    total = totalCount,
                    last30Days = count30,
                    byType = allAnalyses.GroupBy(a => a.AnalysisType)
                                         .Select(g => new { type = g.Key, count = g.Count() })
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
                    total = allAnomalies.Count,
                    unresolved = allAnomalies.Count(a => !a.IsResolved),
                    criticalSeverityCount = allAnomalies.Count(a => a.Severity == "Critical"),
                    highSeverityCount = allAnomalies.Count(a => a.Severity == "High"),
                    mediumSeverityCount = allAnomalies.Count(a => a.Severity == "Medium"),
                    lowSeverityCount = allAnomalies.Count(a => a.Severity == "Low"),

                }
            };

            return new ApiResponse(200, "Statistics retrieved successfully", stats);
        }
    }
}