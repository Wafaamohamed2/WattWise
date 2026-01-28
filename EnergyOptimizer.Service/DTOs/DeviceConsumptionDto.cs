namespace EnergyOptimizer.API.DTOs
{
    public class DeviceConsumptionDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public double RatedPowerKW { get; set; }
        public double CurrentConsumption { get; set; }
        public double TodayConsumption { get; set; }
        public double AverageConsumption { get; set; }
        public int ReadingsCount { get; set; }
        public DateTime? LastReadingTime { get; set; }
        public bool IsActive { get; set; }
    }
}
