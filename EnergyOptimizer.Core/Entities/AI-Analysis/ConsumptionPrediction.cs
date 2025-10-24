using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class ConsumptionPrediction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PredictionDate { get; set; }

        public double PredictedConsumptionKWh { get; set; }

        public double? ActualConsumptionKWh { get; set; }

        public double ConfidenceScore { get; set; }

        public string Explanation { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PredictionType { get; set; } = "Daily"; // Daily, Weekly, Monthly

        public string Metadata { get; set; } = string.Empty; // JSON for additional data

        // Calculate accuracy when actual data is available
        [NotMapped]
        public double? Accuracy
        {
            get
            {
                if (!ActualConsumptionKWh.HasValue || PredictedConsumptionKWh == 0)
                    return null;

                var error = Math.Abs(ActualConsumptionKWh.Value - PredictedConsumptionKWh);
                return Math.Max(0, 100 - (error / PredictedConsumptionKWh * 100));
            }
        }
    }
}
