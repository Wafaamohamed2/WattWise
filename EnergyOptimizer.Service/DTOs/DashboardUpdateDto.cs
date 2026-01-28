namespace EnergyOptimizer.API.DTOs
{
    public class DashboardUpdateDto
    {
        public DateTime Timestamp { get; set; }
        public double TotalConsumption { get; set; }
        public int ActiveDevices { get; set; }
        public int TotalReadings { get; set; }
        public List<TopConsumerDto> TopConsumers { get; set; } = new();
    }

    public class TopConsumerDto
    {
        public int DeviceId { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public double CurrentConsumption { get; set; }
    }
}
