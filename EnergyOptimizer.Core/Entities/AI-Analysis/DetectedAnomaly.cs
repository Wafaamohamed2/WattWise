using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class DetectedAnomaly
    {
        [Key]
        public int Id { get; set; }

        public int? AnalysisId { get; set; }

        [ForeignKey(nameof(AnalysisId))]
        public EnergyAnalysis? Analysis { get; set; }

        [Required]
        public int DeviceId { get; set; }

        [ForeignKey(nameof(DeviceId))]
        public Device Device { get; set; } = null!;

        [Required]
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime AnomalyTimestamp { get; set; }

        public double ActualValue { get; set; }

        public double ExpectedValue { get; set; }

        public double Deviation { get; set; }
        [Required]
        [MaxLength(20)]
        public string Severity { get; set; } = "Medium"; // Low, Medium, High, Critical

        [Required]
        public string Description { get; set; } = string.Empty;

        public string PossibleCauses { get; set; } = string.Empty; // JSON array

        public bool IsResolved { get; set; } = false;

        public DateTime? ResolvedAt { get; set; }

        public string ResolutionNotes { get; set; } = string.Empty;

    }
}
