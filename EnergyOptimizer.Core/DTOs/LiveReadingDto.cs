namespace EnergyOptimizer.API.DTOs
{
    public class LiveReadingDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public decimal PowerConsumptionKW { get; set; }
        public decimal Voltage { get; set; }
        public double Current { get; set; }
        public double Temperature { get; set; }
        public bool IsActive { get; set; }
    }
}
