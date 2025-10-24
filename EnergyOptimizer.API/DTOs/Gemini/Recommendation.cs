namespace EnergyOptimizer.API.DTOs.Gemini
{
    public class Recommendation
    {
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium"; // Low, Medium, High
        public double PotentialSavingsKWh { get; set; }
        public List<string> ActionItems { get; set; } = new();
    }
}
