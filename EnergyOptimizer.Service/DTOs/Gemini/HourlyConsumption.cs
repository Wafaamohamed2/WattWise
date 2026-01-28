namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class HourlyConsumption
    {
        public DateTime Timestamp { get; set; }
        public int Hour { get; set; }
        public double ConsumptionKWh { get; set; }
    }
}
