using EnergyOptimizer.API.DTOs.Gemini;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IPatternDetectionService
    {
        Task<GeminiAnalysisResult> AnalyzeConsumptionPatterns(DateTime start, DateTime end);
        Task<AnomalyDetectionResult> DetectDeviceAnomalies(int deviceId, int days);
        Task<RecommendationResult> GenerateRecommendations(DateTime startDate, DateTime endDate);
        Task<PredictionResult> PredictConsumption(int daysToPredict = 7);
        Task<string> AskQuestion(string question, string context);

    }
}
