
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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
            _logger.LogInformation("Starting AI Global Analysis...");
            await CleanupOldAnalyses(90, ct);
            await CleanupResolvedAnomalies(30, ct);
            await MarkExpiredRecommendations(ct);
        }
        public async Task CleanupOldAnalyses(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            var spec = new CleanupSpecification<EnergyAnalysis>(a => a.AnalysisDate < cutoffDate);
            var oldAnalyses = await _analysisRepo.ListAsync(spec);

            if (oldAnalyses.Any())
            {
                _analysisRepo.DeleteRange(oldAnalyses);
                await _analysisRepo.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old analyses", oldAnalyses.Count);
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
                _alertRepo.DeleteRange(oldAlerts);
                await _alertRepo.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} old alerts", oldAlerts.Count);
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
                foreach (var rec in expiredRecs)
                {
                    rec.Description += " (EXPIRED)";
                }

                _recommendationRepo.UpdateRange(expiredRecs);
                int updatedCount = await _recommendationRepo.SaveChangesAsync();
                _logger.LogInformation("Updated {Count} expired recommendations", updatedCount);
            }
        }




    }


}
