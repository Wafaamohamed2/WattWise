using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.DTOs.DeviceDTOs
{
    public class UpdateDeviceDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ZoneId { get; set; }
        public DeviceType? Type { get; set; }
        public decimal? RatedPowerKW { get; set; }
        public bool? IsActive { get; set; }
    }
}
