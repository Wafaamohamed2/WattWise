namespace EnergyOptimizer.API.DTOs
{
    public class HourlyConsumptionDto
    {
        public int Hour { get; set; }
        public string TimeLabel { get; set; } = string.Empty;
        public decimal TotalConsumption { get; set; }
        public int ReadingsCount { get; set; }
        public decimal AverageConsumption { get; set; }
    }
}
