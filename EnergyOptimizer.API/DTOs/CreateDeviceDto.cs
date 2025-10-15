using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.API.DTOs
{
    public class CreateDeviceDto
    {
        public string Name { get; set; } = string.Empty;
        public int ZoneId { get; set; }
        public DeviceType Type { get; set; }
        public double RatedPowerKW { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? InstallationDate { get; set; }
    }
}
