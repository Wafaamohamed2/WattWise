namespace EnergyOptimizer.API.DTOs
{
    public class ConsumptionTrendDto
    {
        public DateTime Timestamp { get; set; }
        public double TotalConsumption { get; set; }
        public int ActiveDevices { get; set; }
    }
}
