using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class ConsumptionPoint
    {
        [Required]
        public DateTime Timestamp { get; set; }

        [Range(0, 1000, ErrorMessage = "Value out of realistic range")]
        public double ConsumptionKWh { get; set; }
    }
}
