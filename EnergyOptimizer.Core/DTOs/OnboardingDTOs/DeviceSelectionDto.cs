using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.DTOs.OnboardingDTOs
{
    public class DeviceSelectionDto
    {
        public DeviceType DeviceType { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; } = 1;
        public double RatedPowerKW { get; set; } = 0.5;
        public string? ZoneName { get; set; }
        public ZoneType ZoneType { get; set; } = ZoneType.LivingRoom;
    }

    public class CreateBuildingWithTypeDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public BuildingType Type { get; set; } = BuildingType.Home;
    }

    public class CompleteOnboardingDto
    {
        public int BuildingId { get; set; }
        public List<DeviceSelectionDto> Devices { get; set; } = new();
    }
}
