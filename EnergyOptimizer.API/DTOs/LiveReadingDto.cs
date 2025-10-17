namespace EnergyOptimizer.API.DTOs
{
    public class LiveReadingDto
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double PowerConsumptionKW { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double Temperature { get; set; }
        public bool IsActive { get; set; }
    }
}
