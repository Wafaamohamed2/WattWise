using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Service.Services
{
    public class DataSeedingService
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(IGenericRepository<Building> buildingRepo, IGenericRepository<Zone> zoneRepo, IGenericRepository<Device> deviceRepo, ILogger<DataSeedingService> logger)
        {
            _buildingRepo = buildingRepo;
            _zoneRepo = zoneRepo;
            _deviceRepo = deviceRepo;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                // Check if data already exists
                if (await _buildingRepo.GetQueryable().AnyAsync())
                {
                    _logger.LogInformation("Data already seeded. Skipping...");
                    return;
                }

                _logger.LogInformation("Starting data seeding...");

                // Create Building
                var building = new Building
                {
                    Name = "My Smart Home",
                    Address = "123 Main Street, Cairo",
                    TotalArea = 200,
                    NumberOfRooms = 5,
                    CreatedAt = DateTime.UtcNow
                };
                _buildingRepo.Add(building);
                await _buildingRepo.SaveChangesAsync();

                // Create Zones
                var zones = new List<Zone>
                {
                    new Zone { Name = "Master Bedroom", BuildingId = building.Id, Type = ZoneType.Bedroom, Area = 25 },
                    new Zone { Name = "Bedroom 2", BuildingId = building.Id, Type = ZoneType.Bedroom, Area = 20 },
                    new Zone { Name = "Bedroom 3", BuildingId = building.Id, Type = ZoneType.Bedroom, Area = 18 },
                    new Zone { Name = "Living Room", BuildingId = building.Id, Type = ZoneType.LivingRoom, Area = 40 },
                    new Zone { Name = "Kitchen", BuildingId = building.Id, Type = ZoneType.Kitchen, Area = 15 },
                    new Zone { Name = "Bathroom 1", BuildingId = building.Id, Type = ZoneType.Bathroom, Area = 8 },
                    new Zone { Name = "Bathroom 2", BuildingId = building.Id, Type = ZoneType.Bathroom, Area = 6 }
                };
                _zoneRepo.AddRange(zones);
                await _zoneRepo.SaveChangesAsync();

                // Create Devices
                var devices = new List<Device>
                {
                    // Air Conditioners
                    new Device { Name = "AC - Master Bedroom", ZoneId = zones[0].Id, Type = DeviceType.AirConditioner, RatedPowerKW =(decimal) 1.8, IsActive = true },
                    new Device { Name = "AC - Bedroom 2", ZoneId = zones[1].Id, Type = DeviceType.AirConditioner, RatedPowerKW = (decimal) 1.5, IsActive = true },
                    new Device { Name = "AC - Bedroom 3", ZoneId = zones[2].Id, Type = DeviceType.AirConditioner, RatedPowerKW =(decimal)  1.5, IsActive = true },
                    new Device { Name = "AC - Living Room", ZoneId = zones[3].Id, Type = DeviceType.AirConditioner, RatedPowerKW = (decimal) 2.5, IsActive = true },
                    
                    // Kitchen
                    new Device { Name = "Refrigerator", ZoneId = zones[4].Id, Type = DeviceType.Refrigerator, RatedPowerKW = (decimal) 0.15, IsActive = true },
                    new Device { Name = "Microwave", ZoneId = zones[4].Id, Type = DeviceType.Microwave, RatedPowerKW =(decimal)  1.2, IsActive = true },
                    
                    // Water Heater & Washing Machine
                    new Device { Name = "Water Heater", ZoneId = zones[5].Id, Type = DeviceType.WaterHeater, RatedPowerKW = (decimal) 2.5, IsActive = true },
                    new Device { Name = "Washing Machine", ZoneId = zones[5].Id, Type = DeviceType.WashingMachine, RatedPowerKW =(decimal)  1.5, IsActive = true },
                    
                    // TVs
                    new Device { Name = "TV - Living Room", ZoneId = zones[3].Id, Type = DeviceType.TV, RatedPowerKW =(decimal)  0.2, IsActive = true },
                    new Device { Name = "TV - Master Bedroom", ZoneId = zones[0].Id, Type = DeviceType.TV, RatedPowerKW = (decimal) 0.15, IsActive = true },
                    
                    // Lights
                    new Device { Name = "Lights - Master Bedroom", ZoneId = zones[0].Id, Type = DeviceType.Lights, RatedPowerKW =(decimal)  0.06, IsActive = true },
                    new Device { Name = "Lights - Bedroom 2", ZoneId = zones[1].Id, Type = DeviceType.Lights, RatedPowerKW = (decimal) 0.05, IsActive = true },
                    new Device { Name = "Lights - Bedroom 3", ZoneId = zones[2].Id, Type = DeviceType.Lights, RatedPowerKW = (decimal) 0.05, IsActive = true },
                    new Device { Name = "Lights - Living Room", ZoneId = zones[3].Id, Type = DeviceType.Lights, RatedPowerKW =(decimal)  0.1, IsActive = true },
                    new Device { Name = "Lights - Kitchen", ZoneId = zones[4].Id, Type = DeviceType.Lights, RatedPowerKW = (decimal) 0.04, IsActive = true }
                };

                _deviceRepo.AddRange(devices);
                await _deviceRepo.SaveChangesAsync();

                _logger.LogInformation("Data seeding completed successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while seeding data");
                throw;
            }
        }
    }
}