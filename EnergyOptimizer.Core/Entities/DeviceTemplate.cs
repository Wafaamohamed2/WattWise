using System.ComponentModel.DataAnnotations;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Entities
{
    public class DeviceTemplate
    {
        public int Id { get; set; }

        public BuildingType BuildingType { get; set; }

        public DeviceType DeviceType { get; set; }

        [Required]
        [MaxLength(100)]
        public string SuggestedName { get; set; } = string.Empty;

        public int DefaultQuantity { get; set; } = 1;

        public double DefaultRatedPowerKW { get; set; } = 0.5;

        public string? SuggestedZoneName { get; set; }

        public ZoneType SuggestedZoneType { get; set; } = ZoneType.LivingRoom;
    }
}
