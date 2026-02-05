using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.API.DTOs
{
    public class HourlyConsumptionDto
    {
        [Range(0, 23, ErrorMessage = "Hour must be between 0 and 23")]
        public int Hour { get; set; }
        public string TimeLabel { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Consumption cannot be negative")]
        public decimal TotalConsumption { get; set; }
        public int ReadingsCount { get; set; }
        public decimal AverageConsumption { get; set; }
    }
}
