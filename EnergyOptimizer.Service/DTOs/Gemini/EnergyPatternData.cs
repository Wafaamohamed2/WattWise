using EnergyOptimizer.AI.Services;

namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class EnergyPatternData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DevicePattern> DevicePatterns { get; set; } = new();
        public List<HourlyConsumption> HourlyData { get; set; } = new();
        public double TotalConsumptionKWh { get; set; }
    }
}
