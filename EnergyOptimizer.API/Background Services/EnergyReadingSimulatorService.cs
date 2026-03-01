using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Core.DTOs.DashboardDTOs;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EnergyOptimizer.API.Services
{
    public class EnergyReadingSimulatorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EnergyReadingSimulatorService> _logger;
        private readonly IOptionsMonitor<SimulationOptions> _options;
        private readonly IHubContext<EnergyHub> _hubContext;

        public EnergyReadingSimulatorService(IServiceProvider serviceProvider, ILogger<EnergyReadingSimulatorService> logger, IOptionsMonitor<SimulationOptions> options,
              IHubContext<EnergyHub> hubContext)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Energy Reading Simulator Service started");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);


            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateReadingsAsync(stoppingToken);
                    var intervalMinutes = _options.CurrentValue.IntervalMinutes;
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch(OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating energy readings");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

        }
        private async Task GenerateReadingsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            var activeDevices = await context.Devices
                .Where(d => d.IsActive)
                .Include(d => d.Zone)
                .ToListAsync();

            if (!activeDevices.Any())
            {
                _logger.LogWarning("No active devices found for generating readings");
                return;
            }

            var readings = new List<EnergyReading>(activeDevices.Count);
            var liveReadings = new List<LiveReadingDto>(activeDevices.Count);

            var nowUtc = DateTime.UtcNow;
            var cairoTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time");
            var hour = cairoTime.Hour;
            var dayOfWeek = cairoTime.DayOfWeek;

            bool blackout = Random.Shared.NextDouble() < 0.05;

            foreach (var device in activeDevices)
            {
                decimal consumption = CalculateConsumption(device, hour, dayOfWeek);

                var voltage = GenerateVoltage(blackout);
                var current = consumption > 0 ? (consumption / 220.0m) * 1000m : 0m;
                var tempReading = (decimal)GenerateTemperature(hour);

                //  Generate reading even if consumption is 0 (standby mode)
                var reading = new EnergyReading
                {
                    DeviceId = device.Id,
                    Timestamp = cairoTime,
                    PowerConsumptionKW = consumption,
                    Voltage = GenerateVoltage(blackout),
                    Current = (consumption > 0 ? (consumption / 220.0m) * 1000m : 0m),
                    Temperature = GenerateTemperature(hour)
                };

                liveReadings.Add(new LiveReadingDto
                {
                    DeviceId = device.Id,
                    DeviceName = device.Name ?? "Unknown Device",
                    ZoneName = device.Zone?.Name ?? "Unknown Zone",
                    Timestamp = nowUtc,
                    PowerConsumptionKW = Math.Round(consumption, 4),
                    Current = (double)Math.Round(reading.Current, 2),
                    Voltage = Math.Round(reading.Voltage, 2),
                    Temperature = Math.Round(reading.Temperature, 2),
                    IsActive = device.IsActive
                });
            }

            if (readings.Any())
            {
                await context.EnergyReadings.AddRangeAsync(readings);
                await context.SaveChangesAsync();
                _logger.LogInformation("Generated {Count} readings at {Time}" ,readings.Count, cairoTime);

                // Broadcast — fire in parallel for efficiency
                await Task.WhenAll(
                   BroadcastReadingsAsync(liveReadings, activeDevices.Count),
                   BroadcastDashboardUpdateAsync(liveReadings, activeDevices.Count));
            }
        }

        //  Broadcast readings to all clients
        private async Task BroadcastReadingsAsync(List<LiveReadingDto> readings, int totalActiveDevices)
        {
            try
            {
                if (readings == null || readings.Count == 0)
                    return;

                // Send all readings
                await _hubContext.Clients.All.SendAsync("ReceiveReadings", readings);

                // Send zone-specific readings
                var zoneTasks = readings
                    .GroupBy(r => r.ZoneName)
                    .Select(g => _hubContext.Clients
                        .Group($"Zone_{g.Key}")
                        .SendAsync("ReceiveZoneReadings", g.ToList()));

                await Task.WhenAll(zoneTasks);

                _logger.LogDebug($"Broadcasted {readings.Count} readings to all clients");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting readings");
            }
        }

        private async Task BroadcastDashboardUpdateAsync(List<LiveReadingDto> readings , int totalActiveDevices)
        {
            try
            {
                var update = new DashboardUpdateDto
                {
                    Timestamp = DateTime.UtcNow,
                    TotalConsumption = Math.Round(readings.Sum(r => r.PowerConsumptionKW), 2),
                    ActiveDevices = totalActiveDevices,
                    TotalReadings = readings.Count,
                    TopConsumers = readings
                        .OrderByDescending(r => r.PowerConsumptionKW)
                        .Take(5)
                        .Select(r => new TopConsumerDto
                        {
                            DeviceName = r.DeviceName,
                            CurrentConsumption = r.PowerConsumptionKW
                        })
                        .ToList()
                };

                await _hubContext.Clients.All.SendAsync("DashboardUpdate", update);

                _logger.LogDebug("Broadcasted dashboard update - Active: {ActiveDevices}", totalActiveDevices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error broadcasting dashboard update");
            }
        }
        private decimal CalculateConsumption(Device device, int hour, DayOfWeek dayOfWeek)

        {
            var baseConsumption = device.RatedPowerKW;
            var rng = Random.Shared;
            bool isWeekend = (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday);

            return device.Type switch
            {
                DeviceType.AirConditioner when (hour >= 22 || hour <= 6)   // night
                    => baseConsumption * (decimal)(0.7 + rng.NextDouble() * 0.3),

                DeviceType.AirConditioner when (hour >= 13 && hour <= 17)  // afternoon
                    => baseConsumption * (decimal)(0.9 + rng.NextDouble() * 0.3),

                DeviceType.AirConditioner
                    => rng.NextDouble() < 0.1 ? baseConsumption * 0.2m : 0m,

                DeviceType.Refrigerator
                    => baseConsumption * (decimal)(0.15 + rng.NextDouble() * 0.15),

                DeviceType.WashingMachine when ((hour >= 7 && hour <= 9) || (hour >= 18 && hour <= 20))
                    => rng.NextDouble() < 0.3 ? baseConsumption * (decimal)(0.7 + rng.NextDouble() * 0.3) : 0m,

                DeviceType.WashingMachine => 0m,

                DeviceType.WaterHeater when ((hour >= 5 && hour <= 8) || (hour >= 18 && hour <= 22))
                    => baseConsumption * (decimal)(0.6 + rng.NextDouble() * 0.4),

                DeviceType.WaterHeater => baseConsumption * 0.1m,

                DeviceType.Lights when (hour >= 18 && hour <= 23)
                    => baseConsumption * (decimal)(0.8 + rng.NextDouble() * 0.2),

                DeviceType.Lights when (hour >= 6 && hour <= 8)
                    => baseConsumption * (decimal)(0.5 + rng.NextDouble() * 0.3),

                DeviceType.Lights => 0m,

                DeviceType.TV when (hour >= 18 && hour <= 23)
                    => baseConsumption * (decimal)(0.7 + rng.NextDouble() * 0.3),

                DeviceType.TV when (isWeekend && hour >= 12 && hour <= 17)
                    => baseConsumption * (decimal)(0.6 + rng.NextDouble() * 0.3),

                DeviceType.TV => 0m,

                DeviceType.Microwave when ((hour >= 7 && hour <= 9) || (hour >= 12 && hour <= 14) || (hour >= 19 && hour <= 21))
                    => rng.NextDouble() < 0.15 ? baseConsumption * (decimal)(0.5 + rng.NextDouble() * 0.5) : 0m,

                DeviceType.Microwave => 0m,

                _ => baseConsumption * (decimal)(rng.NextDouble() * 0.5)
            };
        }

        private decimal GenerateVoltage(bool blackout)
        {
            // Egypt standard voltage is around 220V
            if (blackout) return 180 + (decimal)Random.Shared.NextDouble() * 10; 
            return 220 + (decimal)Random.Shared.NextDouble() * 20 - 10;
        }

        private double GenerateTemperature(int hour)
        {
            if (hour >= 12 && hour <= 17)
                return 28 + Random.Shared.NextDouble() * 6;
            else if (hour >= 22 || hour <= 6)
                return 22 + Random.Shared.NextDouble() * 4;
            else
                return 24 + Random.Shared.NextDouble() * 5;
        }

        public class SimulationOptions
        {
            public const string SectionName = "Simulation";
            public int IntervalMinutes { get; set; } = 1;
        }
    }
}
