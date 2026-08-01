using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.DTOs.ZoneDTOs
{
    public class ZoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public ZoneType Type { get; set; }
        public double Area { get; set; }
    }

    public class CreateZoneDto
    {
        public string Name { get; set; } = string.Empty;
        public int BuildingId { get; set; }
        public ZoneType Type { get; set; }
        public double Area { get; set; }
    }

    public class UpdateZoneDto
    {
        public string Name { get; set; } = string.Empty;
        public ZoneType? Type { get; set; }
        public double? Area { get; set; }
    }
}
