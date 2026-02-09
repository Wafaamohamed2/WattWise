namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class ConsumptionSummary
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public double TotalConsumptionKWh { get; set; }
        public double AverageDailyConsumption { get; set; }
        public List<DeviceSummary> DeviceSummaries { get; set; } = new();
        public List<string> CurrentIssues { get; set; } = new();
    }
}
