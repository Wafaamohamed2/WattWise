namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class RecommendationResult
    {
        public List<Recommendation> Recommendations { get; set; } = new();
        public double EstimatedSavingsKWh { get; set; }
        public double EstimatedSavingsPercent { get; set; }
    }
}
