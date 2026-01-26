
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Services
{
    public class DataCleanupService : IDataCleanupService
    {
        private readonly EnergyDbContext _context;
        private readonly ILogger<DataCleanupService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DataCleanupService(EnergyDbContext context, ILogger<DataCleanupService> logger, IServiceProvider serviceProvider)
        {
            _context = context;
            _logger = logger;
            _serviceProvider = serviceProvider;
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

            int deletedCount = await _context.EnergyAnalyses
                .Where(a => a.AnalysisDate < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Database Cleanup: Removed {Count} old analysis records older than {Date}",
                    deletedCount, cutoffDate.ToShortDateString());
            }
        }

        public async Task CleanupResolvedAnomalies(int daysToKeep, CancellationToken cancellationToken)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

            int deletedCount = await _context.Alerts
                .Where(a => a.Type == AlertType.Anomaly && a.IsRead && a.CreatedAt < cutoffDate)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleanup: Deleted {Count} resolved anomalies", deletedCount);
            }
        }

        public async Task MarkExpiredRecommendations(CancellationToken cancellationToken)
        {
            int updatedCount = await _context.EnergyRecommendations
                .Where(r => !r.IsImplemented && r.ExpiresAt < DateTime.UtcNow)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Description, r => r.Description + " (EXPIRED)"),
             cancellationToken);

            if (updatedCount > 0)
            {
                _logger.LogInformation("Updated {Count} expired recommendations", updatedCount);
            }
        }


    

    }

    
}
