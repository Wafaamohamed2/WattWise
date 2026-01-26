
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.API.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIAnalysisService> _logger;

        public AIAnalysisService(IServiceProvider serviceProvider, ILogger<AIAnalysisService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task RunGlobalAnalysisAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting AI Global Analysis...");
            await RunDailyAnalysis(ct);
            await DetectAnomalies(ct);
            await GenerateRecommendations(ct);
        }

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
    }
}

     
    

