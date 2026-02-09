namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class DailyConsumption
    {
        public DateTime Date { get; set; }
        public double ConsumptionKWh { get; set; }
        public double Temperature { get; set; }
        public bool IsWeekend { get; set; }
    }
}
