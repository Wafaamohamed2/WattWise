using EnergyOptimizer.API.DTOs;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Services
{

    public class AlertDetectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertDetectionService> _logger;
        private readonly IHubContext<EnergyHub> _hubContext;

        public AlertDetectionService(
             IServiceProvider serviceProvider,
             ILogger<AlertDetectionService> logger,
             IHubContext<EnergyHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Alert Detection Service started");

            // Wait for app to start
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DetectAlertsAsync();

                    // Check every 30 seconds
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Alert Detection Service");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }

            }
        }

        private async Task DetectAlertsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            var now = DateTime.UtcNow;
            var fiveMinutesAgo = now.AddMinutes(-5);
            var oneMinuteAgo = now.AddMinutes(-1);

            // Get all active devices with recent readings
            var devices = await context.Devices
                .Include(d => d.Zone)
                .Include(d => d.EnergyReadings
                    .Where(r => r.Timestamp >= fiveMinutesAgo)
                    .OrderByDescending(r => r.Timestamp))
                .Where(d => d.IsActive)
                .ToListAsync();

            var alertsToCreate = new List<Alert>();

            foreach (var device in devices)
            {
                var recentReadings = device.EnergyReadings.ToList();

                if (!recentReadings.Any())
                {
                    // Device Offline Alert
                    var offlineAlert = await CheckDeviceOffline(context, device, now);
                    if (offlineAlert != null)
                        alertsToCreate.Add(offlineAlert);

                    continue;
                }
                var latestReading = recentReadings.First();

                // 1. High Consumption Alert
                var highConsumptionAlert = CheckHighConsumption(device, latestReading, recentReadings);
                if (highConsumptionAlert != null)
                    alertsToCreate.Add(highConsumptionAlert);

                // 2. Anomaly Detection Alert
                var anomalyAlert = CheckAnomaly(device, latestReading, recentReadings);
                if (anomalyAlert != null)
                    alertsToCreate.Add(anomalyAlert);

                // 3. Wastage Detection Alert
                var wastageAlert = CheckWastage(device, latestReading, now);
                if (wastageAlert != null)
                    alertsToCreate.Add(wastageAlert);
            }

            // Save new alerts to database and broadcast via SignalR
            if (alertsToCreate.Any())
            {
                await context.Alerts.AddRangeAsync(alertsToCreate);
                await context.SaveChangesAsync();

                // Broadcast each alert
                foreach (var alert in alertsToCreate)
                {
                    await BroadcastAlert(context, alert);

                }

                _logger.LogInformation("Created {Count} new alerts", alertsToCreate.Count);
            }
        }

        // 1. High Consumption Detection
        private Alert? CheckHighConsumption(Device device, EnergyReading latestReading, List<EnergyReading> recentReadings)
        {
            // Calculate baseline (average of last 5 readings, excluding latest)
            var historicalReadings = recentReadings.Skip(1).Take(5).ToList();

            if (historicalReadings.Count < 3)
                return null; // Not enough data

            var baseline = historicalReadings.Average(r => r.PowerConsumptionKW);
            var threshold = baseline * 1.5; // 150% of baseline

            if (latestReading.PowerConsumptionKW > threshold && latestReading.PowerConsumptionKW > 0.5)
            {
                return new Alert
                {
                    DeviceId = device.Id,
                    Type = AlertType.HighConsumption,
                    Severity = 2, // Warning
                    Message = $"{device.Name} is consuming {latestReading.PowerConsumptionKW:F2} kW (expected: ~{baseline:F2} kW). This is {((latestReading.PowerConsumptionKW / baseline - 1) * 100):F0}% higher than normal.",
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };
            }

            return null;
        }

        // 2. Anomaly Detection (Sudden Spikes/Drops)
        private Alert? CheckAnomaly(Device device, EnergyReading latestReading, List<EnergyReading> recentReadings)
        {
            if (recentReadings.Count < 3)
                return null;

            var previousReading = recentReadings.Skip(1).FirstOrDefault();
            if (previousReading == null)
                return null;

            // Check for sudden spike (200% increase in 1 minute)
            if (previousReading.PowerConsumptionKW > 0 &&
                latestReading.PowerConsumptionKW > previousReading.PowerConsumptionKW * 2 &&
                latestReading.PowerConsumptionKW > 1.0)
            {
                return new Alert
                {
                    DeviceId = device.Id,
                    Type = AlertType.Anomaly,
                    Severity = 3, // Critical
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

            var hour = now.Hour;
            var isNightTime = hour >= 0 && hour <= 5; // 12 AM - 5 AM

            switch (device.Type)
            {
                case DeviceType.TV:
                    if (isNightTime && latestReading.PowerConsumptionKW > 0.1)
                    {
                        return new Alert
                        {
                            DeviceId = device.Id,
                            Type = AlertType.Wastage,
                            Severity = 2, // Warning
                            Message = $"{device.Name} is ON at {now:HH:mm}. Consider turning it off to save energy.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                    }
                    break;
                case DeviceType.Lights:
                    if (isNightTime && latestReading.PowerConsumptionKW > 0.05)
                    {
                        return new Alert
                        {
                            DeviceId = device.Id,
                            Type = AlertType.Wastage,
                            Severity = 1, // Info
                            Message = $"{device.Name} is still ON at {now:HH:mm}. Remember to turn off lights when not needed.",
                            CreatedAt = DateTime.UtcNow,
                            IsRead = false
                        };
                    }
                    break;
                case DeviceType.WashingMachine:
                    // Washing machine running at night (energy saving tip)
                    if (hour >= 23 || hour <= 6)
                    {
                        if (latestReading.PowerConsumptionKW > 0.5)
                        {
                            return new Alert
                            {
                                DeviceId = device.Id,
                                Type = AlertType.Wastage,
                                Severity = 1, // Info
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
        private async Task<Alert?> CheckDeviceOffline(EnergyDbContext context, Device device, DateTime now)
        {
            // Skip check if device is intentionally turned off
            if (!device.IsActive)
                return null;

            // Check if there's already an offline alert for this device
            var existingAlert = await context.Alerts
                .Where(a => a.DeviceId == device.Id &&
                           a.Type == AlertType.DeviceOffline &&
                           a.CreatedAt >= now.AddMinutes(-10))
                .AnyAsync();

            if (existingAlert)
                return null; // Don't create duplicate alerts

            return new Alert
            {
                DeviceId = device.Id,
                Type = AlertType.DeviceOffline,
                Severity = 2, // Warning
                Message = $"{device.Name} has not reported any readings in the last 5 minutes. Device may be offline or malfunctioning.",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
        }

        // Broadcast Alert via SignalR
        private async Task BroadcastAlert(EnergyDbContext context, Alert alert)
        {
            try
            {
                var device = await context.Devices
                   .Include(d => d.Zone)
                   .FirstOrDefaultAsync(d => d.Id == alert.DeviceId);

                if (device == null)
                    return;

                var alertDto = new AlertDto
                {
                    Id = alert.Id,
                    DeviceName = device.Name,
                    ZoneName = device.Zone.Name,
                    AlertType = alert.Type.ToString(),
                    Message = alert.Message,
                    Severity = alert.Severity,
                    SeverityLabel = alert.Severity switch
                    {
                        1 => "Info",
                        2 => "Warning",
                        3 => "Critical",
                        _ => "Unknown"
                    },

                    Icon = alert.Severity switch
                    {
                        1 => "🔵",
                        2 => "🟡",
                        3 => "🔴",
                        _ => "⚪"
                    },
                    CreatedAt = alert.CreatedAt,
                    IsRead = alert.IsRead
                };
                await _hubContext.Clients.All.SendAsync("NewAlert", alertDto);

                _logger.LogInformation("Broadcasted alert {AlertId} for device {DeviceName}", alert.Id, device.Name, alert.Message);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting alert {AlertId}", alert.Id);
            }
        }
    }
}

