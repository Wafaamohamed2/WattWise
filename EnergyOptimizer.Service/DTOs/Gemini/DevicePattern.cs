namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class DevicePattern
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public double AverageConsumptionKWh { get; set; }
        public double PeakConsumptionKWh { get; set; }
        public int ActiveHours { get; set; }
        public List<int> PeakHours { get; set; } = new();
    }
}
