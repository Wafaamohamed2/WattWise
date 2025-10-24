using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class EnergyRecommendation
    {
        [Key]
        public int Id { get; set; }

        public int? AnalysisId { get; set; }

        [ForeignKey(nameof(AnalysisId))]
        public EnergyAnalysis? Analysis { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // DeviceOptimization, ScheduleAdjustment, etc.

        [Required]
        [MaxLength(20)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public double EstimatedSavingsKWh { get; set; }

        public double EstimatedSavingsPercent { get; set; }
        public string ActionItems { get; set; } = string.Empty; // JSON array

        public bool IsImplemented { get; set; } = false;

        public DateTime? ImplementedDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }
    }
}
