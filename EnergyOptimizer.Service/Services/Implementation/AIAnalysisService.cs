using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AnomaliesSpec;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Service.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Service.Services.Implementation
{
    public class AIAnalysisService : IAIAnalysisService
    {
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;
        private readonly ILogger<AIAnalysisService> _logger;

        public AIAnalysisService(
            IGenericRepository<Device> deviceRepo,
            IGenericRepository<EnergyReading> readingRepo,
            IGenericRepository<Alert> alertRepo,
            IGenericRepository<EnergyAnalysis> analysisRepo,
            IGenericRepository<EnergyRecommendation> recommendationRepo,
            IGenericRepository<DetectedAnomaly> anomalyRepo,
            ILogger<AIAnalysisService> logger)
        {
            _deviceRepo = deviceRepo;
            _readingRepo = readingRepo;
            _alertRepo = alertRepo;
            _analysisRepo = analysisRepo;
            _recommendationRepo = recommendationRepo;
            _anomalyRepo = anomalyRepo;
            _logger = logger;
        }

        public async Task RunGlobalAnalysisAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting AI Global Analysis...");

            await RunDailyAnalysis(ct);
            await DetectAnomalies(ct);
            await GenerateRecommendations(ct);

            _logger.LogInformation("AI Global Analysis completed successfully");
        }

        private async Task DetectAnomalies(CancellationToken cancellationToken)
        {
            var activeDevices = await _deviceRepo.ListAsync(new ActiveDevicesWithZoneSpec(true));

            foreach (var device in activeDevices)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var readings = await _readingRepo.ListAsync(
                    new ReadingsByDeviceAndDateSpec(
                        device.Id,
                        DateTime.UtcNow.AddDays(-7),
                        DateTime.UtcNow));

                if (!readings.Any()) continue;

                decimal avg = readings.Average(r => r.PowerConsumptionKW);
                decimal stdDev = CalculateStandardDeviation(
                    readings.Select(r => r.PowerConsumptionKW));

                decimal threshold = avg + 2 * stdDev;

                var anomalies = readings
                    .Where(r => r.PowerConsumptionKW > threshold)
                    .ToList();

                foreach (var r in anomalies)
                {
                    // Check if anomaly already exists for this timestamp to avoid duplicates
                    var alreadyExists = await _anomalyRepo.AnyAsync(
                        new AnomalyExistsSpec(device.Id, r.Timestamp));

                    if (alreadyExists) continue;

                    decimal deviationPercent =
                        avg == 0 ? 0 : (r.PowerConsumptionKW - avg) / avg * 100;

                    string severity =
                        Math.Abs(deviationPercent) > 60 ? "Critical" : "High";

                    var anomaly = new DetectedAnomaly
                    {
                        DeviceId = device.Id,
                        AnomalyTimestamp = r.Timestamp,
                        ActualValue = (double)r.PowerConsumptionKW,
                        ExpectedValue = (double)Math.Round(avg, 2),
                        Deviation = (double)Math.Abs(Math.Round(deviationPercent, 1)),
                        Severity = severity,
                        Description =
                            $"Usage is {Math.Abs(Math.Round(deviationPercent, 1))}% away from average",
                        DetectedAt = DateTime.UtcNow,
                        IsResolved = false
                    };

                    _anomalyRepo.Add(anomaly);

                    _alertRepo.Add(new Alert
                    {
                        DeviceId = device.Id,
                        Type = Core.Enums.AlertType.Anomaly,
                        Severity = severity == "Critical" ? AlertSeverity.Critical : AlertSeverity.Warning,
                        Message = $"AI Alert: {device.Name} anomaly detected",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            await _anomalyRepo.SaveChangesAsync();
            await _alertRepo.SaveChangesAsync();
        }


        private async Task RunDailyAnalysis(CancellationToken ct)
        {
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;

            var readings = await _readingRepo.ListAsync(
                new ReadingsByDateRangeSpec(startDate, endDate));

            if (!readings.Any()) return;

            double totalConsumption =
                (double)readings.Sum(r => r.PowerConsumptionKW);

            int deviceCount = readings
                .Select(r => r.DeviceId)
                .Distinct()
                .Count();

            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = "Global",
                PeriodStart = startDate,
                PeriodEnd = DateTime.UtcNow,
                Summary = $"Analysis completed for {deviceCount} devices.",
                TotalConsumptionKWh = totalConsumption,
                DevicesAnalyzed = deviceCount,
                FullResponse = "{}"
            };

            _analysisRepo.Add(analysis);
            await _analysisRepo.SaveChangesAsync();
        }


        private async Task GenerateRecommendations(CancellationToken ct)
        {
            var devices = await _deviceRepo.ListAsync(new HighPowerDevicesSpec(2.0m));

            var recommendations = devices.Select(device => new EnergyRecommendation
            {
                Title = $"Optimize {device.Name}",
                Category = "Efficiency",
                Priority = "1",
                EstimatedSavingsKWh = 10.5,
                EstimatedSavingsPercent = 15.0,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsImplemented = false,
                Description = $"Usage pattern optimization for {device.Name}.",
                ActionItems = "Review schedule; Check device health;"
            }).ToList();

            if (recommendations.Any())
            {
                _recommendationRepo.AddRange(recommendations);
                await _recommendationRepo.SaveChangesAsync();
            }
        }

        private decimal CalculateStandardDeviation(IEnumerable<decimal> values)
        {
            var list = values.ToList();
            if (list.Count < 2) return 0;

            decimal avg = list.Average();
            decimal variance =
                list.Sum(v => (v - avg) * (v - avg)) / list.Count;

            return (decimal)Math.Sqrt((double)variance);
        }
    }
}
