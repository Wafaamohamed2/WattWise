using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class UsagePattern
    {
        [Key]
        public int Id { get; set; }

        public int? DeviceId { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public Device? Device { get; set; }

        public int? ZoneId { get; set; }

        [ForeignKey(nameof(ZoneId))]
        public Zone? Zone { get; set; }

        [Required]
        [MaxLength(100)]
        public string PatternName { get; set; } = string.Empty; // e.g., "Evening Peak", "Weekend Usage"

        [Required]
        [MaxLength(50)]
        public string PatternType { get; set; } = string.Empty; // Hourly, Daily, Weekly

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public double AverageConsumption { get; set; }

        public double PeakConsumption { get; set; }

        public string PeakHours { get; set; } = string.Empty; // JSON array of hours

        public int Frequency { get; set; } // How often this pattern occurs

        public double Confidence { get; set; } // 0-1 confidence score

        public bool IsActive { get; set; } = true;
    }
}
