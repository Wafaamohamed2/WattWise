namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class DeviceSummary
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public double ConsumptionKWh { get; set; }
        public double PercentageOfTotal { get; set; }
        public int DaysActive { get; set; }
    }
}
