using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.Core.Entities
{
    public class Building
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        public double TotalArea { get; set; }

        public int NumberOfRooms { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<Zone> Zones { get; set; } = new List<Zone>();
    }
}