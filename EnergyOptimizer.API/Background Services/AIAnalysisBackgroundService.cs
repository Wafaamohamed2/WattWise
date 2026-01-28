using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.API.Services
{
    public class AIAnalysisBackgroundService : BackgroundService
    {
        private readonly ILogger<AIAnalysisBackgroundService> _logger;
        private readonly IConfiguration _config;
        private readonly IServiceProvider _serviceProvider;

        public AIAnalysisBackgroundService(
            ILogger<AIAnalysisBackgroundService> logger,
            IConfiguration config,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _config = config;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalHours = _config.GetValue<int>("AIAnalysis:IntervalHours", 24);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var aiService = scope.ServiceProvider.GetRequiredService<IAIAnalysisService>();
                    await aiService.RunGlobalAnalysisAsync(stoppingToken);

                    var cleanupService = scope.ServiceProvider.GetRequiredService<IDataCleanupService>();
                    await cleanupService.RunAllCleanupTasks(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in AI background cycle");
                }

                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }
        }


    }
}