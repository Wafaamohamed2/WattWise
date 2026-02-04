using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using AnomalyEntity = EnergyOptimizer.Core.Entities.AI_Analysis.DetectedAnomaly;
using AnomalyDto = EnergyOptimizer.API.DTOs.Gemini.DetectedAnomaly;
using EnergyOptimizer.API.DTOs.Gemini;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Service.Services;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.API.Services
{
    // Service to detect energy consumption patterns and prepare data for AI analysis
    public class PatternDetectionService
    {
        private readonly EnergyDbContext _context;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<PatternDetectionService> _logger;

        public PatternDetectionService(
            EnergyDbContext context,
            IGeminiService geminiService,
            ILogger<PatternDetectionService> logger)
        {
            _context = context;
            _geminiService = geminiService;
            _logger = logger;
        }

        // Analyze energy patterns for a specific time period
        public async Task<GeminiAnalysisResult> AnalyzeConsumptionPatterns(
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                _logger.LogInformation("Starting pattern analysis from {Start} to {End}",
                    startDate, endDate);

                // ===== 1. Collect data from database =====
                var readings = await _context.EnergyReadings
                    .Include(r => r.Device)
                    .ThenInclude(d => d.Zone)
                    .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
                    .ToListAsync();

                if (!readings.Any())
                {
                    _logger.LogWarning("No readings found for the specified period");
                    return new GeminiAnalysisResult
                    {
                        Success = false,
                        ErrorMessage = "No data available for analysis"
                    };
                }

                // ===== 2. Transform to AI input DTO =====
                var energyPatternData = TransformToEnergyPatternData(readings, startDate, endDate);

                // ===== 3. Call Gemini AI =====
                var analysisResult = await _geminiService.AnalyzeEnergyPatterns(energyPatternData);

                // ===== 4. Save results to database =====
                if (analysisResult.Success)
                {
                    await SaveAnalysisResults(analysisResult, startDate, endDate, readings.Count);
                }

                return analysisResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing consumption patterns");
                return new GeminiAnalysisResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Detect anomalies for a specific device
        public async Task<AnomalyDetectionResult> DetectDeviceAnomalies(
            int deviceId,
            int daysToAnalyze = 7)
        {
            try
            {
                var device = await _context.Devices
                    .FirstOrDefaultAsync(d => d.Id == deviceId);

                if (device == null)
                {
                    return new AnomalyDetectionResult
                    {
                        HasAnomalies = false,
                        Analysis = "Device not found"
                    };
                }

                var startDate = DateTime.UtcNow.AddDays(-daysToAnalyze);
                var readings = await _context.EnergyReadings
                    .Where(r => r.DeviceId == deviceId && r.Timestamp >= startDate)
                    .OrderBy(r => r.Timestamp)
                    .ToListAsync();

                if (readings.Count < 10)
                {
                    return new AnomalyDetectionResult
                    {
                        HasAnomalies = false,
                        Analysis = "Insufficient data for analysis"
                    };
                }

                // Transform to DTO
                var consumptionData = TransformToDeviceConsumptionData(device, readings);

                // Call AI
                var result = await _geminiService.DetectAnomalies(consumptionData);

                // Save anomalies if found
                if (result.HasAnomalies && result.Anomalies.Any())
                {
                    await SaveDetectedAnomalies(result.Anomalies, deviceId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalies for device {DeviceId}", deviceId);
                return new AnomalyDetectionResult
                {
                    HasAnomalies = false,
                    Analysis = $"Error: {ex.Message}"
                };
            }
        }

        // Generate energy-saving recommendations
        public async Task<RecommendationResult> GenerateRecommendations(
            DateTime startDate,
            DateTime endDate)
        {
            try
            {
                // Collect summary data
                var readings = await _context.EnergyReadings
                    .Include(r => r.Device)
                    .Where(r => r.Timestamp >= startDate && r.Timestamp <= endDate)
                    .ToListAsync();

                if (!readings.Any())
                {
                    return new RecommendationResult();
                }

                // Get current issues (unresolved anomalies and alerts)
                var currentIssues = new List<string>();

                var unresolvedAnomalies = await _context.DetectedAnomalies
                    .Where(a => !a.IsResolved && a.DetectedAt >= startDate)
                    .CountAsync();

                if (unresolvedAnomalies > 0)
                {
                    currentIssues.Add($"{unresolvedAnomalies} unresolved anomalies detected");
                }

                var unreadAlerts = await _context.Alerts
                    .Where(a => !a.IsRead && a.CreatedAt >= startDate)
                    .CountAsync();

                if (unreadAlerts > 0)
                {
                    currentIssues.Add($"{unreadAlerts} unread alerts");
                }

                // Transform to DTO
                var summary = TransformToConsumptionSummary(readings, startDate, endDate, currentIssues);

                // Call AI
                var result = await _geminiService.GenerateRecommendations(summary);

                // Save recommendations
                if (result.Recommendations.Any())
                {
                    await SaveRecommendations(result.Recommendations);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
                return new RecommendationResult();
            }
        }

        // Predict future consumption
        public async Task<PredictionResult> PredictConsumption(int daysToPredict = 7)
        {
            try
            {
                var historicalDays = 30;
                var startDate = DateTime.UtcNow.AddDays(-historicalDays);

                var dailyData = await _context.EnergyReadings
                    .Where(r => r.Timestamp >= startDate)
                    .GroupBy(r => r.Timestamp.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Consumption = g.Sum(r => r.PowerConsumptionKW),
                        AvgTemp = g.Average(r => r.Temperature)
                    })
                    .OrderBy(d => d.Date)
                    .ToListAsync();

                if (dailyData.Count < 7)
                {
                    return new PredictionResult
                    {
                        PredictionDate = DateTime.UtcNow.AddDays(1),
                        ConfidenceScore = 0,
                        Explanation = "Insufficient historical data for prediction"
                    };
                }

                // Transform to DTO
                var historicalData = new HistoricalData
                {
                    DaysToPredict = daysToPredict,
                    DailyConsumptions = dailyData.Select(d => new DailyConsumption
                    {
                        Date = d.Date,
                        ConsumptionKWh = (double)d.Consumption,
                        Temperature = d.AvgTemp,
                        IsWeekend = d.Date.DayOfWeek == DayOfWeek.Friday ||
                                  d.Date.DayOfWeek == DayOfWeek.Saturday
                    }).ToList()
                };

                // Call AI
                var result = await _geminiService.PredictConsumption(historicalData);

                // Save prediction
                if (result.ConfidenceScore > 0)
                {
                    await SavePrediction(result);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting consumption");
                return new PredictionResult
                {
                    PredictionDate = DateTime.UtcNow.AddDays(1),
                    ConfidenceScore = 0,
                    Explanation = $"Error: {ex.Message}"
                };
            }
        }

        //  PRIVATE TRANSFORMATION METHODS 

        private EnergyPatternData TransformToEnergyPatternData(
            List<EnergyReading> readings,
            DateTime startDate,
            DateTime endDate)
        {
            var deviceGroups = readings.GroupBy(r => r.Device);

            var devicePatterns = deviceGroups.Select(group =>
            {
                var device = group.Key;
                var deviceReadings = group.ToList();

                var avgConsumption = deviceReadings.Average(r => r.PowerConsumptionKW);
                var peakConsumption = deviceReadings.Max(r => r.PowerConsumptionKW);

                // Find peak hours
                var hourlyAvg = deviceReadings
                    .GroupBy(r => r.Timestamp.Hour)
                    .Select(g => new { Hour = g.Key, Avg = g.Average(r => r.PowerConsumptionKW) })
                    .OrderByDescending(h => h.Avg)
                    .ToList();

                var threshold = avgConsumption * (decimal)1.2;
                var peakHours = hourlyAvg
                    .Where(h => h.Avg > threshold)
                    .Select(h => h.Hour)
                    .OrderBy(h => h)
                    .ToList();

                return new DevicePattern
                {
                    DeviceName = device.Name,
                    DeviceType = device.Type.ToString(),
                    AverageConsumptionKWh = (double)Math.Round(avgConsumption, 4),
                    PeakConsumptionKWh = (double)Math.Round(peakConsumption, 4),
                    ActiveHours = deviceReadings.Select(r => r.Timestamp.Hour).Distinct().Count(),
                    PeakHours = peakHours
                };
            }).ToList();

            // Hourly aggregation
            var hourlyData = readings
                .GroupBy(r => new { r.Timestamp.Date, r.Timestamp.Hour })
                .Select(g => new HourlyConsumption
                {
                    Timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    Hour = g.Key.Hour,
                    ConsumptionKWh = (double)Math.Round(g.Sum(r => r.PowerConsumptionKW), 4)
                })
                .OrderBy(h => h.Timestamp)
                .ToList();

            return new EnergyPatternData
            {
                StartDate = startDate,
                EndDate = endDate,
                DevicePatterns = devicePatterns,
                HourlyData = hourlyData,
                TotalConsumptionKWh = (double)Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2)
            };
        }

        private DeviceConsumptionData TransformToDeviceConsumptionData(
            Device device,
            List<EnergyReading> readings)
        {
            var consumptions = readings.Select(r => r.PowerConsumptionKW).ToList();
            var avg = consumptions.Average();
            var variance = consumptions.Average(c => Math.Pow((double)(c - avg), 2));
            var stdDev = Math.Sqrt(variance);

            return new DeviceConsumptionData
            {
                DeviceName = device.Name,
                AverageConsumption = (double)Math.Round(avg, 4),
                StandardDeviation = Math.Round(stdDev, 4),
                ConsumptionHistory = readings.Select(r => new ConsumptionPoint
                {
                    Timestamp = r.Timestamp,
                    ConsumptionKWh = (double)Math.Round(r.PowerConsumptionKW, 4)
                }).ToList()
            };
        }

        private ConsumptionSummary TransformToConsumptionSummary(
            List<EnergyReading> readings,
            DateTime startDate,
            DateTime endDate,
            List<string> currentIssues)
        {
            var totalDays = (endDate - startDate).Days + 1;
            var totalConsumption = readings.Sum(r => r.PowerConsumptionKW);

            var deviceSummaries = readings
                .GroupBy(r => r.Device)
                .Select(g => new DeviceSummary
                {
                    DeviceName = g.Key.Name,
                    DeviceType = g.Key.Type.ToString(),
                    ConsumptionKWh = (double)Math.Round(g.Sum(r => r.PowerConsumptionKW), 2),
                    PercentageOfTotal = (double)Math.Round(g.Sum(r => r.PowerConsumptionKW) / totalConsumption * 100, 1),
                    DaysActive = g.Select(r => r.Timestamp.Date).Distinct().Count()
                })
                .OrderByDescending(d => d.ConsumptionKWh)
                .ToList();

            return new ConsumptionSummary
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalConsumptionKWh = (double)Math.Round(totalConsumption, 2),
                AverageDailyConsumption = (double)Math.Round(totalConsumption / totalDays, 2),
                DeviceSummaries = deviceSummaries,
                CurrentIssues = currentIssues
            };
        }

        #region//  PRIVATE SAVE METHODS 

        private async Task SaveAnalysisResults(
            GeminiAnalysisResult result,
            DateTime startDate,
            DateTime endDate,
            int readingsCount)
        {
            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                AnalysisType = "Pattern",
                Summary = result.Summary,
                FullResponse = string.Join("\n", result.Insights),
                DevicesAnalyzed = readingsCount,
                Insights = result.Insights.Select((text, index) => new AnalysisInsight
                {
                    InsightText = text,
                    Category = "Pattern",
                    Priority = index < 2 ? 1 : 2
                }).ToList()
            };

            await _context.EnergyAnalyses.AddAsync(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved analysis results with {InsightCount} insights",
                result.Insights.Count);
        }

        private async Task SaveDetectedAnomalies(
            List<AnomalyDto> anomalies,
            int deviceId)
        {
            foreach (var anomaly in anomalies)
            {
                var entity = new AnomalyEntity
                {
                    DeviceId = deviceId,
                    DetectedAt = DateTime.UtcNow,
                    AnomalyTimestamp = anomaly.Timestamp,
                    ActualValue = anomaly.ActualValue,
                    ExpectedValue = anomaly.ExpectedValue,
                    Deviation = anomaly.Deviation,
                    Severity = anomaly.Severity,
                    Description = anomaly.Description
                };

                await _context.DetectedAnomalies.AddAsync(entity);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} anomalies for device {DeviceId}",
                anomalies.Count, deviceId);
        }

        private async Task SaveRecommendations(List<Recommendation> recommendations)
        {
            foreach (var rec in recommendations)
            {
                var entity = new EnergyRecommendation
                {
                    Title = rec.Title,
                    Description = rec.Description,
                    Category = rec.Category,
                    Priority = rec.Priority,
                    EstimatedSavingsKWh = rec.PotentialSavingsKWh,
                    ActionItems = System.Text.Json.JsonSerializer.Serialize(rec.ActionItems),
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };

                await _context.EnergyRecommendations.AddAsync(entity);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Saved {Count} recommendations", recommendations.Count);
        }

        private async Task SavePrediction(PredictionResult result)
        {
            var prediction = new ConsumptionPrediction
            {
                CreatedAt = DateTime.UtcNow,
                PredictionDate = result.PredictionDate,
                PredictedConsumptionKWh = result.PredictedConsumptionKWh,
                ConfidenceScore = result.ConfidenceScore,
                Explanation = result.Explanation,
                PredictionType = "Daily"
            };

            await _context.ConsumptionPredictions.AddAsync(prediction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved prediction for {Date}", result.PredictionDate);
        }
        #endregion
    }
}