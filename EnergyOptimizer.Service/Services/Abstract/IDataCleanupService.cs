namespace EnergyOptimizer.Service.Services.Abstract
{
    public interface IDataCleanupService
    {
        Task RunAllCleanupTasks(CancellationToken ct);
        Task CleanupOldAnalyses(int daysToKeep, CancellationToken cancellationToken);
        Task CleanupResolvedAnomalies(int daysToKeep, CancellationToken cancellationToken);
        Task MarkExpiredRecommendations(CancellationToken cancellationToken);
    }
}
