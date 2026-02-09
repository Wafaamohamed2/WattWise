namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class DeviceConsumptionData
    {
        public string DeviceName { get; set; } = string.Empty;
        public List<ConsumptionPoint> ConsumptionHistory { get; set; } = new();
        public double AverageConsumption { get; set; }
        public double StandardDeviation { get; set; }
    }
}
