using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class EnergyAnalysis
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PeriodStart { get; set; }

        [Required]
        public DateTime PeriodEnd { get; set; }

        [Required]
        [MaxLength(50)]
        public string AnalysisType { get; set; } = string.Empty; // Pattern, Anomaly, Prediction

        [Required]
        public string Summary { get; set; } = string.Empty;

        public string FullResponse { get; set; } = string.Empty;

        public double TotalConsumptionKWh { get; set; }

        public int DevicesAnalyzed { get; set; }

        // Relationships
        public List<AnalysisInsight> Insights { get; set; } = new();
        public List<EnergyRecommendation> Recommendations { get; set; } = new();
        public List<DetectedAnomaly> Anomalies { get; set; } = new();
    }
}
