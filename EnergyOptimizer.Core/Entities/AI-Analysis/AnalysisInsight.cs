using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class AnalysisInsight
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AnalysisId { get; set; }

        [ForeignKey(nameof(AnalysisId))]
        public EnergyAnalysis Analysis { get; set; } = null!;

        [Required]
        public string InsightText { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // Usage, Efficiency, Cost, etc.

        public int Priority { get; set; } = 3; // 1=High, 2=Medium, 3=Low

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
