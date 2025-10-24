using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Entities.AI_Analysis
{
    public class AIMetrics
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string MetricType { get; set; } = string.Empty; // Prediction, Anomaly, Pattern

        public int TotalRequests { get; set; }

        public int SuccessfulRequests { get; set; }

        public int FailedRequests { get; set; }

        public double AverageResponseTimeMs { get; set; }

        public double AverageCost { get; set; }

        public int CacheHits { get; set; }

        public int CacheMisses { get; set; }

        [NotMapped]
        public double SuccessRate => TotalRequests > 0
            ? (double)SuccessfulRequests / TotalRequests * 100
            : 0;

        [NotMapped]
        public double CacheHitRate => (CacheHits + CacheMisses) > 0
            ? (double)CacheHits / (CacheHits + CacheMisses) * 100
            : 0;
    }


}

