using System.ComponentModel.DataAnnotations;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Entities
{
    public class Device
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int ZoneId { get; set; }

        public DeviceType Type { get; set; }

        public decimal RatedPowerKW { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime InstallationDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Zone Zone { get; set; } = null!;
        public virtual ICollection<EnergyReading> EnergyReadings { get; set; } = new List<EnergyReading>();
    }
}