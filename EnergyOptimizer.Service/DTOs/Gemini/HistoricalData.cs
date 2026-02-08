namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class HistoricalData
    {
        public List<DailyConsumption> DailyConsumptions { get; set; } = new();
        public int DaysToPredict { get; set; } = 7;
    }
}
