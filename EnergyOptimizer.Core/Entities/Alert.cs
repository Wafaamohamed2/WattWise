using System.ComponentModel.DataAnnotations;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Entities
{
    public class Alert
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public AlertType Type { get; set; }

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        public int Severity { get; set; } // 1-5

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual Device Device { get; set; } = null!;
    }
}