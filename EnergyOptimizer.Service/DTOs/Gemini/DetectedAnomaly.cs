namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class DetectedAnomaly
    {
        public string DeviceName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public double ActualValue { get; set; }
        public double ExpectedValue { get; set; }
        public double Deviation { get; set; }
        public string Severity { get; set; } = "Medium"; // Low, Medium, High
        public string Description { get; set; } = string.Empty;
    }
}
