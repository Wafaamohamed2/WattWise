namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class AnomalyDetectionResult
    {
        public bool HasAnomalies { get; set; }
        public List<DetectedAnomaly> Anomalies { get; set; } = new();
        public string Analysis { get; set; } = string.Empty;
    }
}
