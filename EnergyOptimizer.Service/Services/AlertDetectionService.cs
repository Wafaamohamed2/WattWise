using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using Microsoft.Extensions.Options;

namespace EnergyOptimizer.Service.Services
{

    public class AlertDetectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IOptionsMonitor<AlertSettings> _alertSettingsMonitor;
        private readonly ILogger<AlertDetectionService> _logger;

        public AlertDetectionService(
             IServiceProvider serviceProvider,
             IOptionsMonitor<AlertSettings> alertSettingsMonitor,
             ILogger<AlertDetectionService> logger)
        {
            _serviceProvider = serviceProvider;
            _alertSettingsMonitor = alertSettingsMonitor;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert Detection Service started");

            // Wait for app to start
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DetectAlertsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Alert Detection Service");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

            }
        }

        private async Task DetectAlertsAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var deviceRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Device>>();
            var alertRepo = scope.ServiceProvider.GetRequiredService<IGenericRepository<Alert>>();
            var hubService = scope.ServiceProvider.GetRequiredService<IEnergyHubService>();

            var now = DateTime.UtcNow;
            var fiveMinutesAgo = now.AddMinutes(-5);

            var devices = await deviceRepo.ListAsync(new DevicesWithRecentReadingsSpec(fiveMinutesAgo));

            var alertsToCreate = new List<(Alert alert, string deviceName, string zoneName)>();


            foreach (var device in devices)
            {
                if (ct.IsCancellationRequested) break;

                var recentReadings = device.EnergyReadings.ToList();
                var deviceName = device.Name;
                var zoneName = device.Zone?.Name ?? "General";

                if (!recentReadings.Any())
                {
                    var offlineAlert = await CheckDeviceOffline(alertRepo, device, now);
                    if (offlineAlert != null)
                        alertsToCreate.Add((offlineAlert, deviceName, zoneName));
                    continue;
                }
                var latestReading = recentReadings.First();

                var highConsumptionAlert = CheckHighConsumption(device, latestReading, recentReadings);
                if (highConsumptionAlert != null) alertsToCreate.Add((highConsumptionAlert, deviceName, zoneName));

                var anomalyAlert = CheckAnomaly(device, latestReading, recentReadings);
                if (anomalyAlert != null) alertsToCreate.Add((anomalyAlert, deviceName, zoneName));

                var wastageAlert = CheckWastage(device, latestReading, now);
                if (wastageAlert != null) alertsToCreate.Add((wastageAlert, deviceName, zoneName));
            }

            if (!alertsToCreate.Any()) return;

            foreach (var (alert, _, _) in alertsToCreate)
                alertRepo.Add(alert);

            await alertRepo.SaveChangesAsync();

            foreach (var (alert, deviceName, zoneName) in alertsToCreate)
            {
                _logger.LogInformation("New Alert Detected: {AlertType} for {DeviceName} in {ZoneName} - {Message}",
                    alert.Type, deviceName, zoneName, alert.Message);

                await BroadcastAlert(hubService, alert, deviceName, zoneName);
            }
        }

        // 1. High Consumption Detection
        private Alert? CheckHighConsumption(Device device, EnergyReading latestReading, List<EnergyReading> recentReadings)
        {
            var alertSettings = _alertSettingsMonitor.CurrentValue;
            var historicalReadings = recentReadings.Skip(1).Take(alertSettings.HighConsumptionDetection.MinReadingsCount).ToList();

            if (historicalReadings.Count < alertSettings.HighConsumptionDetection.MinReadingsCount)
                return null; 

            var baseline = historicalReadings.Average(r => r.PowerConsumptionKW);
            var threshold = baseline * alertSettings.HighConsumptionDetection.MultiplicationFactor; 

            if (latestReading.PowerConsumptionKW > threshold && latestReading.PowerConsumptionKW > alertSettings.HighConsumptionDetection.MinConsumptionKW)
            {
                return new Alert
                {
                    DeviceId = device.Id,
                    Type = AlertType.HighConsumption,
                    Severity = AlertSeverity.Warning, 
                    Message = $"{device.Name} is consuming {latestReading.PowerConsumptionKW:F2} kW (expected: ~{baseline:F2} kW). This is {((latestReading.PowerConsumptionKW / baseline - 1) * 100):F0}% higher than normal.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
            }

            return null;
        }

        // 2. Anomaly Detection 
        private Alert? CheckAnomaly(Device device, EnergyReading latestReading, List<EnergyReading> recentReadings)
        {
            if (recentReadings.Count < 3)
                return null;

            var previousReading = recentReadings.Skip(1).FirstOrDefault();
            if (previousReading == null)
                return null;

            var alertSettings = _alertSettingsMonitor.CurrentValue;

            // Check for sudden spike (200% increase in 1 minute)
            if (previousReading.PowerConsumptionKW > 0 &&
                latestReading.PowerConsumptionKW > previousReading.PowerConsumptionKW * 2 &&
                latestReading.PowerConsumptionKW > (decimal)1.0)
            {
                return new Alert
                {
                    DeviceId = device.Id,
                    Type = AlertType.Anomaly,
                    Severity = AlertSeverity.Critical,
                    Message = $"{device.Name} consumption spiked from {previousReading.PowerConsumptionKW:F2} kW to {latestReading.PowerConsumptionKW:F2} kW. Please check the device.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
            }

            return null;
        }

        // 3. Wastage Detection (Device running at unusual times)
        private Alert? CheckWastage(Device device, EnergyReading latestReading, DateTime now)
        {
            var alertSettings = _alertSettingsMonitor.CurrentValue;
            var hour = now.Hour;
            var isNightTime = hour >= alertSettings.WastageDetection.NoUseTimeRangeStart || hour <= alertSettings.WastageDetection.NoUseTimeRangeEnd; 

            switch (device.Type)
            {
                case DeviceType.TV:
                    if (isNightTime && (double) latestReading.PowerConsumptionKW > alertSettings.WastageDetection.ThresholdKW)
                    {
                        return new Alert
                        {
                            DeviceId = device.Id,
                            Type = AlertType.Wastage,
                            Severity = AlertSeverity.Warning, 
                            Message = $"{device.Name} is ON at {now:HH:mm}. Consider turning it off to save energy.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                    }
                    break;
                case DeviceType.Lights:
                    if (isNightTime && (double)latestReading.PowerConsumptionKW > alertSettings.WastageDetection.ThresholdKW)
                    {
                        return new Alert
                        {
                            DeviceId = device.Id,
                            Type = AlertType.Wastage,
                            Severity = AlertSeverity.Info,
                            Message = $"{device.Name} is still ON at {now:HH:mm}. Remember to turn off lights when not needed.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                    }
                    break;
                case DeviceType.WashingMachine:
                    if (hour >= alertSettings.WastageDetection.NoUseTimeRangeStart || hour <= alertSettings.WastageDetection.NoUseTimeRangeEnd)
                    {
                        if ((double)latestReading.PowerConsumptionKW > alertSettings.WastageDetection.WashingMachineThresholdKW)
                        {
                            return new Alert
                            {
                                DeviceId = device.Id,
                                Type = AlertType.Wastage,
                                Severity = AlertSeverity.Info, 
                                Message = $"{device.Name} is running during off-peak hours. Good job on saving energy costs!",
                                CreatedAt = DateTime.UtcNow,
                                IsRead = false
                            };
                        }
                    }
                    break;

            }
            return null;
        }


        // 4. Device Offline Detection
        private async Task<Alert?> CheckDeviceOffline(IGenericRepository<Alert> alertRepo, Device device, DateTime now)
        {
            // Skip check if device is intentionally turned off
            if (!device.IsActive)
                return null;

            var tenMinutesAgo = now.AddMinutes(-10);
            var hasExistingAlert = await alertRepo.
                AnyAsync(new AlertOfflineCheckSpec(device.Id, tenMinutesAgo));

            if (hasExistingAlert)
                return null; 

            return new Alert
            {
                DeviceId = device.Id,
                Type = AlertType.DeviceOffline,
                Severity = AlertSeverity.Warning,
                Message = $"{device.Name} has not reported any readings in the last 5 minutes. Device may be offline or malfunctioning.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
        }

        // Broadcast Alert via SignalR
        private async Task BroadcastAlert(IEnergyHubService hubService,Alert alert,string deviceName,string zoneName)
        {     
                try
                {
                    var alertDto = new AlertDto
                    {
                        Id = alert.Id,
                        DeviceName = deviceName,
                        ZoneName = zoneName,
                        AlertType = alert.Type.ToString(),
                        Message = alert.Message,
                        Severity =alert.Severity,
                        CreatedAt = alert.CreatedAt,
                        IsRead = alert.IsRead
                    };

                    await hubService.SendAlertNotification(JsonSerializer.Serialize(alertDto));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error broadcasting alert {AlertId}", alert.Id);
                }
        }

    }
 }


