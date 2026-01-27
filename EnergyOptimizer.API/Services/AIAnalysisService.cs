
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.API.Services
{
    public class AIAnalysisService : IAIAnalysisService
    {

        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Alert> _alertRepo; 
        private readonly ILogger<AIAnalysisService> _logger;

        public AIAnalysisService(
             IGenericRepository<Device> deviceRepo,
             IGenericRepository<EnergyReading> readingRepo,
             IGenericRepository<Alert> alertRepo,
             ILogger<AIAnalysisService> logger)
        {
            _deviceRepo = deviceRepo;
            _readingRepo = readingRepo;
            _alertRepo = alertRepo;
            _logger = logger;
        }

        public async Task RunGlobalAnalysisAsync(CancellationToken ct)
        {
            _logger.LogInformation("Starting AI Global Analysis...");
            await RunDailyAnalysis(ct);
            await DetectAnomalies(ct);
            await GenerateRecommendations(ct);
            await _deviceRepo.SaveChangesAsync();
        }

        private async Task DetectAnomalies(CancellationToken cancellationToken)
        {
            var deviceSpec = new ActiveDevicesWithZoneSpec();
            var activeDevices = await _deviceRepo.ListAsync(deviceSpec);

            int anomaliesFound = 0;
            foreach (var activeDevice in activeDevices)
            {
                if (cancellationToken.IsCancellationRequested) break;

                var readingSpec = new ReadingsByDeviceAndDateSpec(activeDevice.Id, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);
                var readings = await _readingRepo.ListAsync(readingSpec);

                if (readings.Any()) { 
                    var avg = readings.Average(r => r.PowerConsumptionKW);
                    var anomalySpec = new AnomalyReadingsSpec(activeDevice.Id, avg, 0.5);

                    if (await _readingRepo.AnyAsync(anomalySpec))
                    {
                        await _alertRepo.AddAsync(new Alert
                        {
                            DeviceId = activeDevice.Id,
                            Message = $"Unusual consumption detected for {activeDevice.Name}",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
       
        }

        private async Task RunDailyAnalysis(CancellationToken cancellationToken)
        {
            var spec = new TodayReadingsSpec();
            var todayData = await _readingRepo.ListAsync(spec);
        }

        private async Task GenerateRecommendations(CancellationToken cancellationToken)
        {
            var highPowerSpec = new HighPowerDevicesSpec(minPowerKW: 2.0);
            var targetDevices = await _deviceRepo.ListAsync(highPowerSpec);

            _logger.LogInformation("Generating recommendations for last 30 days");

            foreach (var device in targetDevices) { 
                
                await _alertRepo.AddAsync(new Alert
                {
                    DeviceId = device.Id,
                    Message = $"Consider optimizing usage of {device.Name} to reduce energy consumption.",
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
    }
}

     
    

