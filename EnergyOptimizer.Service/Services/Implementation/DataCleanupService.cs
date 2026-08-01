using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using Microsoft.Extensions.Logging;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Service.Services.Abstract;

namespace EnergyOptimizer.Service.Services.Implementation
{
    public class DataCleanupService : IDataCleanupService
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly ILogger<DataCleanupService> _logger;

        public DataCleanupService(
            IGenericRepository<EnergyReading> readingRepo,
            IGenericRepository<Alert> alertRepo,
            IGenericRepository<EnergyRecommendation> recRepo,
            IGenericRepository<EnergyAnalysis> analysisRepo,
            ILogger<DataCleanupService> logger)
        {
            _readingRepo = readingRepo;
            _alertRepo = alertRepo;
            _recommendationRepo = recRepo;
            _analysisRepo = analysisRepo;
            _logger = logger;
        }

        // This method runs all cleanup tasks sequentially
        public async Task RunAllCleanupTasks(CancellationToken ct)
        {
            _logger.LogInformation("Starting data cleanup tasks...");
            await CleanupOldAnalyses(90, ct);
            await CleanupResolvedAnomalies(30, ct);
            await MarkExpiredRecommendations(ct);
            _logger.LogInformation("Data cleanup tasks completed");
        }

        public async Task CleanupOldAnalyses(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var spec = new CleanupSpecification<EnergyAnalysis>(a => a.AnalysisDate < cutoffDate);
            var oldAnalyses = await _analysisRepo.ListAsync(spec);

            if (oldAnalyses.Any())
            {
                const int batchSize = 15;
                for (int i = 0; i < oldAnalyses.Count; i += batchSize)
                {
                    var batch = oldAnalyses.Skip(i).Take(batchSize).ToList();
                    _analysisRepo.DeleteRange(batch);
                    await _analysisRepo.SaveChangesAsync();
                }
                _logger.LogInformation("Deleted {Count} old analyses in batches", oldAnalyses.Count);
            }
        }

        public async Task CleanupResolvedAnomalies(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var spec = new CleanupSpecification<Alert>(a =>
                  a.Type == AlertType.Anomaly &&
                  a.IsRead &&
                  a.CreatedAt < cutoffDate);

            var oldAlerts = await _alertRepo.ListAsync(spec);

            if (oldAlerts.Any())
            {
                const int batchSize = 15;
                for (int i = 0; i < oldAlerts.Count; i += batchSize)
                {
                    var batch = oldAlerts.Skip(i).Take(batchSize).ToList();
                    _alertRepo.DeleteRange(batch);
                    await _alertRepo.SaveChangesAsync();
                }
                _logger.LogInformation("Deleted {Count} old alerts in batches", oldAlerts.Count);
            }
        }

        public async Task MarkExpiredRecommendations(CancellationToken cancellationToken)
        {
            var spec = new CleanupSpecification<EnergyRecommendation>(r =>
                  !r.IsImplemented &&
                  r.ExpiresAt < DateTime.UtcNow);

            var expiredRecs = await _recommendationRepo.ListAsync(spec);

            if (expiredRecs.Any())
            {
                const int batchSize = 15;
                int totalUpdated = 0;

                for (int i = 0; i < expiredRecs.Count; i += batchSize)
                {
                    var batch = expiredRecs.Skip(i).Take(batchSize).ToList();
                    foreach (var rec in batch)
                    {
                        rec.ExpiresAt = DateTime.UtcNow;
                    }

                    _recommendationRepo.UpdateRange(batch);
                    totalUpdated += await _recommendationRepo.SaveChangesAsync();
                }

                _logger.LogInformation("Updated {Count} expired recommendations in batches", totalUpdated);
            }
        }
    }
}
