using EnergyOptimizer.API.DTOs.Gemini;

namespace EnergyOptimizer.Service.Services
{
    public interface IGeminiService
    {
        // Analyze energy consumption patterns and provide insights
        Task<GeminiAnalysisResult> AnalyzeEnergyPatterns(EnergyPatternData data);

        // Detect anomalies in device consumption
        Task<AnomalyDetectionResult> DetectAnomalies(DeviceConsumptionData data);

        // Generate energy-saving recommendations
        Task<RecommendationResult> GenerateRecommendations(ConsumptionSummary summary);

        // Ask a general question about energy optimization
        Task<string> AskQuestion(string question, string context);

        // Predict future consumption based on historical data
        Task<PredictionResult> PredictConsumption(HistoricalData data);
    }

}