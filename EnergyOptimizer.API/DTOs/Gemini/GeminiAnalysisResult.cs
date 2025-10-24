namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class GeminiAnalysisResult
    {
        public bool Success { get; set; }
        public string Summary { get; set; } = string.Empty;
        public List<string> Insights { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public Dictionary<string, double> Metrics { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
