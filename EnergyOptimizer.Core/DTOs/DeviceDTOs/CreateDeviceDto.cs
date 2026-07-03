using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.DTOs.DeviceDTOs
{
    public class CreateDeviceDto
    {
        public string Name { get; set; }
        public int ZoneId { get; set; }
        public DeviceType Type { get; set; }
        public decimal RatedPowerKW { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? InstallationDate { get; set; }
    }
}
