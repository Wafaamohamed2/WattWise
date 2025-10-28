using EnergyOptimizer.API.Services;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.API.Services
{  
    public class AIAnalysisBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIAnalysisBackgroundService> _logger;
        private readonly IConfiguration _config;

        public AIAnalysisBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<AIAnalysisBackgroundService> logger,
            IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Background Service started");

            // Wait 30 seconds before starting
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

            // Run every 24 hours (configurable)
            var intervalHours = _config.GetValue<int>("AIAnalysis:IntervalHours", 24);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting automatic AI analysis cycle");

                    await RunDailyAnalysis(stoppingToken);
                    await DetectAnomalies(stoppingToken);
                    await GenerateRecommendations(stoppingToken);
                    await CleanupOldData(stoppingToken);

                    _logger.LogInformation("AI analysis cycle completed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AI analysis cycle");
                }

                // Wait for next cycle
                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }

            _logger.LogInformation("AI Analysis Background Service stopped");
        }

        // Analyze patterns for the last 7 days
        private async Task RunDailyAnalysis(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var patternService = scope.ServiceProvider.GetRequiredService<PatternDetectionService>();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-7);

                _logger.LogInformation("Analyzing patterns from {Start} to {End}",
                    startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

                var result = await patternService.AnalyzeConsumptionPatterns(startDate, endDate);

                if (result.Success)
                {
                    _logger.LogInformation("Pattern analysis completed: {InsightCount} insights, {RecommendationCount} recommendations",
                        result.Insights.Count, result.Recommendations.Count);
                }
                else
                {
                    _logger.LogWarning("Pattern analysis failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running daily analysis");
            }
        }

        // Detect anomalies for all active devices
        private async Task DetectAnomalies(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var patternService = scope.ServiceProvider.GetRequiredService<PatternDetectionService>();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            try
            {
                var activeDevices = await context.Devices
                    .Where(d => d.IsActive)
                    .Select(d => d.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Checking {Count} devices for anomalies", activeDevices.Count);

                int anomaliesFound = 0;

                foreach (var deviceId in activeDevices)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        var result = await patternService.DetectDeviceAnomalies(deviceId, daysToAnalyze: 7);

                        if (result.HasAnomalies && result.Anomalies.Any())
                        {
                            anomaliesFound += result.Anomalies.Count;
                            _logger.LogWarning("Device {DeviceId}: {Count} anomalies detected",
                                deviceId, result.Anomalies.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error detecting anomalies for device {DeviceId}", deviceId);
                    }

                    // Small delay between devices to avoid overloading on API calls
                    await Task.Delay(1000, cancellationToken);
                }

                _logger.LogInformation("Anomaly detection completed: {Count} anomalies found", anomaliesFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in anomaly detection");
            }
        }

        // Generate recommendations for the last 30 days
        private async Task GenerateRecommendations(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var patternService = scope.ServiceProvider.GetRequiredService<PatternDetectionService>();

            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddDays(-30);

                _logger.LogInformation("Generating recommendations for last 30 days");

                var result = await patternService.GenerateRecommendations(startDate, endDate);

                if (result.Recommendations.Any())
                {
                    _logger.LogInformation("Generated {Count} recommendations, estimated savings: {Savings:F2} kWh",
                        result.Recommendations.Count, result.EstimatedSavingsKWh);
                }
                else
                {
                    _logger.LogWarning("No recommendations generated");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
            }
        }

        // Cleanup old analysis data (keep last 90 days)
        private async Task CleanupOldData(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-90);

                // Delete old analyses
                var oldAnalyses = await context.EnergyAnalyses
                    .Where(a => a.AnalysisDate < cutoffDate)
                    .ToListAsync(cancellationToken);

                if (oldAnalyses.Any())
                {
                    context.EnergyAnalyses.RemoveRange(oldAnalyses);
                    await context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Cleaned up {Count} old analyses", oldAnalyses.Count);
                }

                // Delete resolved anomalies older than 30 days
                var oldAnomalies = await context.DetectedAnomalies
                    .Where(a => a.IsResolved && a.ResolvedAt < DateTime.UtcNow.AddDays(-30))
                    .ToListAsync(cancellationToken);

                if (oldAnomalies.Any())
                {
                    context.DetectedAnomalies.RemoveRange(oldAnomalies);
                    await context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Cleaned up {Count} old resolved anomalies", oldAnomalies.Count);
                }

                // Mark expired recommendations
                var expiredRecommendations = await context.EnergyRecommendations
                    .Where(r => !r.IsImplemented && r.ExpiresAt < DateTime.UtcNow)
                    .ToListAsync(cancellationToken);

                if (expiredRecommendations.Any())
                {
                    _logger.LogInformation(" {Count} recommendations expired", expiredRecommendations.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up old data");
            }
        }
    }
}