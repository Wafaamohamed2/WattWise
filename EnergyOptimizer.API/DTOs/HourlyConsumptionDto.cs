namespace EnergyOptimizer.API.DTOs
{
    public class HourlyConsumptionDto
    {
        public int Hour { get; set; }
        public string TimeLabel { get; set; } = string.Empty;
        public double TotalConsumption { get; set; }
        public int ReadingsCount { get; set; }
        public double AverageConsumption { get; set; }
    }
}
