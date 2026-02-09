namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class PredictionResult
    {
        public DateTime PredictionDate { get; set; }
        public double PredictedConsumptionKWh { get; set; }
        public double ConfidenceScore { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
