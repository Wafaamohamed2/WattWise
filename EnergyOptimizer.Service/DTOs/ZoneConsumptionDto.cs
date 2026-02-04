namespace EnergyOptimizer.API.DTOs
{
    public class ZoneConsumptionDto
    {
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string ZoneType { get; set; } = string.Empty;
        public int DeviceCount { get; set; }
        public decimal TotalConsumption { get; set; }
        public decimal AverageConsumption { get; set; }
        public decimal Percentage { get; set; }
    }
}
