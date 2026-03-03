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
            var today = DateTime.UtcNow;
            var thirtyDaysAgo = today.AddDays(-30);
            var sevenDaysAgo = today.AddDays(-7);

            // Analyses
            var totalAnalysesTask = _analysisRepo.CountAsync(new AnalysisHistoryCountSpec(null, null, null));
            var recentAnalysesTask = _analysisRepo.CountAsync(new AnalysisHistoryCountSpec(null, thirtyDaysAgo, null));

            // Recommendations
            var activeRecsTask = _recommendationRepo.CountAsync(new RecommendationsCountSpec(isImplemented: false));
            var doneRecsTask = _recommendationRepo.CountAsync(new RecommendationsCountSpec(isImplemented: true));

            var implementedRecsTask = _recommendationRepo.ListAsync(new RecommendationsFilterSpec(isImplemented: true));
            var activeRecsListTask = _recommendationRepo.ListAsync(new RecommendationsFilterSpec(isImplemented: false));

            // Anomalies
            var totalAnomaliesTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, null, null));
            var unresolvedTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(isResolved: false, null, null));
            var recentAnomaliesTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, null, null));
            var criticalTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, "Critical", null));
            var highTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, "High", null));
            var mediumTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, "Medium", null));
            var lowTask = _anomalyRepo.CountAsync(new AnomaliesCountSpec(null, "Low", null));


            // Run all DB queries in parallel
            await Task.WhenAll(
                totalAnalysesTask, recentAnalysesTask,
                activeRecsTask, doneRecsTask, implementedRecsTask, activeRecsListTask,
                totalAnomaliesTask, unresolvedTask, recentAnomaliesTask,
                criticalTask, highTask, mediumTask, lowTask);

            var implementedRecs = await implementedRecsTask;
            var activeRecsList = await activeRecsListTask;

            var stats = new
            {
                analyses = new
                {
                    total = await totalAnalysesTask,
                    last30Days = await recentAnalysesTask
                },
                recommendations = new
                {
                    active = await activeRecsTask,
                    implemented = await doneRecsTask,
                    totalRealizedSavings = implementedRecs.Sum(r => r.EstimatedSavingsKWh),
                    totalPotentialSavings = activeRecsList.Sum(r => r.EstimatedSavingsKWh)
                },
                anomalies = new
                {
                    total = await totalAnomaliesTask,
                    unresolved = await unresolvedTask,
                    criticalSeverityCount = await criticalTask,
                    highSeverityCount = await highTask,
                    mediumSeverityCount = await mediumTask,
                    lowSeverityCount = await lowTask
                }
            };

            return new ApiResponse(200, "Statistics retrieved successfully", stats);
        }
    }
}