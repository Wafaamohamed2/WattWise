namespace EnergyOptimizer.Service.Services.Abstract
{
    public interface IAIAnalysisService
    {
        Task RunGlobalAnalysisAsync(CancellationToken ct);
    }
}
