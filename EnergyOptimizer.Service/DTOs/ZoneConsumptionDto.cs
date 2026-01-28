namespace EnergyOptimizer.API.DTOs
{
    public class ZoneConsumptionDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string ZoneType { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
        public double TotalConsumption { get; set; }
        public double AverageConsumption { get; set; }
        public double Percentage { get; set; }
    }
}
