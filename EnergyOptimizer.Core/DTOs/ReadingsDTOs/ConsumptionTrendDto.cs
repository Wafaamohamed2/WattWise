using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.Core.DTOs.ReadingsDTOs
{
    public class ConsumptionTrendDto
    {
        public DateTime Timestamp { get; set; }
        [Range(0, double.MaxValue)]
        public decimal TotalConsumption { get; set; }
        public int ActiveDevices { get; set; }
    }
}
