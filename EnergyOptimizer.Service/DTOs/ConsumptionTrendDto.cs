namespace EnergyOptimizer.API.DTOs
{
    public class ConsumptionTrendDto
    {
        public DateTime Timestamp { get; set; }
        public decimal TotalConsumption { get; set; }
        public int ActiveDevices { get; set; }
    }
}
