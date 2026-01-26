namespace EnergyOptimizer.API.Services
{
    public interface IAIAnalysisService
    {
        Task RunGlobalAnalysisAsync(CancellationToken ct);
    }
}
