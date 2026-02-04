using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.API.DTOs
{
    public class UpdateDeviceDto
    {
        public string? Name { get; set; }
        public int? ZoneId { get; set; }
        public DeviceType? Type { get; set; }
        public decimal? RatedPowerKW { get; set; }
        public bool? IsActive { get; set; }
    }
}
