using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Services
{
    public class EnergyReadingSimulatorService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EnergyReadingSimulatorService> _logger;
        private readonly IConfiguration _config;

        public EnergyReadingSimulatorService(IServiceProvider serviceProvider, ILogger<EnergyReadingSimulatorService> logger, IConfiguration config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Energy Reading Simulator Service started");

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            var intervalMinutes = _config.GetValue<int>("Simulation:IntervalMinutes", 1);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await GenerateReadingsAsync();
                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating energy readings");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

        }
        private async Task GenerateReadingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

            var activeDevices = await context.Devices
                .Where(d => d.IsActive)
                .ToListAsync();

            if (!activeDevices.Any())
            {
                _logger.LogWarning("No active devices found for generating readings");
                return;
            }

            var readings = new List<EnergyReading>();

            var cairoTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Egypt Standard Time");
            var hour = cairoTime.Hour;
            var dayOfWeek = cairoTime.DayOfWeek;
            var random =  Random.Shared;

            bool blackout = random.NextDouble() < 0.05;

            foreach (var device in activeDevices)
            {
                double consumption = CalculateConsumption(device, hour, dayOfWeek);

                if (consumption >0)
                {
                    var reading = new EnergyReading
                    {
                        DeviceId = device.Id,
                        Timestamp = cairoTime,
                        PowerConsumptionKW = consumption,
                        Voltage = GenerateVoltage(blackout),
                        Current = consumption / 220.0 * 1000,
                        Temperature = GenerateTemperature(hour)
                    };
                    readings.Add(reading);
                }

            }

            if (readings.Any())
            {
                await context.EnergyReadings.AddRangeAsync(readings);
                await context.SaveChangesAsync();
                _logger.LogInformation($"Generated {readings.Count} readings at {cairoTime:yyyy-MM-dd HH:mm:ss}");

            }
        }
        private double CalculateConsumption(Device device, int hour, DayOfWeek dayOfWeek)

        {
            var baseConsumption = device.RatedPowerKW;
            var random = Random.Shared;
            bool isWeekend = (dayOfWeek == DayOfWeek.Friday || dayOfWeek == DayOfWeek.Saturday);

            switch (device.Type)
            {
                case DeviceType.AirConditioner:
                    // AC works during hot hours (12PM-5PM) and night (10PM-6AM)

                    if ((hour >= 22 || hour <= 6))
                        // Night time usage
                        return baseConsumption * (0.7 + random.NextDouble() * 0.3);

                    else if (hour >= 13 && hour <= 17)
                        // Afternoon usage
                        return baseConsumption * (0.9 + random.NextDouble() * 0.3);

                    else
                        // Off or minimal
                        return random.NextDouble() < 0.1 ? baseConsumption * 0.2 : 0;


                case DeviceType.Refrigerator:
                    // Fridge runs continuously with slight variations
                    return baseConsumption * (0.15 + random.NextDouble() * 0.15);

                case DeviceType.WashingMachine:
                    // Washing machine used mainly in mornings and evenings
                    if ((hour >= 7 && hour <= 9) || (hour >= 18 && hour <= 20))
                        return random.NextDouble() < 0.3 ? baseConsumption * (0.7 + random.NextDouble() * 0.3) : 0;
                    return 0;

                case DeviceType.WaterHeater:
                    // Water heater used in early mornings and evenings
                    if ((hour >= 5 && hour <= 8) || (hour >= 18 && hour <= 22))
                        return baseConsumption * (0.6 + random.NextDouble() * 0.4);
                    return baseConsumption * 0.1;

                case DeviceType.Lights:
                    // Lights used mainly in evenings and nights
                    if (hour >= 18 && hour <= 23)
                        return baseConsumption * (0.8 + random.NextDouble() * 0.2);
                    else if (hour >= 6 && hour <= 8)
                        return baseConsumption * (0.5 + random.NextDouble() * 0.3);
                    return 0;

                case DeviceType.TV:
                    // TV used mainly in evenings and weekends
                    if (hour >= 18 && hour <= 24)
                        return baseConsumption * (0.7 + random.NextDouble() * 0.3);
                    else if (isWeekend && hour >= 12 && hour <= 17)
                        return baseConsumption * (0.6 + random.NextDouble() * 0.3);
                    return 0;

                case DeviceType.Microwave:
                    if ((hour >= 7 && hour <= 9) || (hour >= 12 && hour <= 14) || (hour >= 19 && hour <= 21))
                        return random.NextDouble() < 0.15 ? baseConsumption * (0.5 + random.NextDouble() * 0.5) : 0;
                    return 0;

                default:
                    return baseConsumption * random.NextDouble() * 0.5;
            }
        }

        private double GenerateVoltage(bool blackout)
        {
            // Egypt standard voltage is around 220V
            if (blackout) return 180 + Random.Shared.NextDouble() * 10; 
            return 220 + Random.Shared.NextDouble() * 20 - 10;
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
    }
}
