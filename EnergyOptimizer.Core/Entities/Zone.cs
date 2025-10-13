using System.ComponentModel.DataAnnotations;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Entities
{
    public class Zone
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public int BuildingId { get; set; }

        public ZoneType Type { get; set; }

        public double Area { get; set; }

        // Navigation Properties
        public virtual Building Building { get; set; } = null!;
        public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
    }
}