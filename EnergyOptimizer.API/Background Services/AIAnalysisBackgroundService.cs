using EnergyOptimizer.Service.Services.Abstract;
using Microsoft.Extensions.Options;

namespace EnergyOptimizer.API.Services
{
    public class AIAnalysisBackgroundService : BackgroundService
    {
        private readonly ILogger<AIAnalysisBackgroundService> _logger;
        private readonly IOptionsMonitor<AIAnalysisOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        public AIAnalysisBackgroundService(
            ILogger<AIAnalysisBackgroundService> logger,
            IOptionsMonitor<AIAnalysisOptions> options,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _options = options;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var aiService = scope.ServiceProvider.GetRequiredService<IAIAnalysisService>();
                    await aiService.RunGlobalAnalysisAsync(stoppingToken);

                    var cleanupService = scope.ServiceProvider.GetRequiredService<IDataCleanupService>();
                    await cleanupService.RunAllCleanupTasks(stoppingToken);

                    _logger.LogInformation("AI analysis cycle completed successfully");
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in AI background cycle");
                }

                var intervalHours = _options.CurrentValue.IntervalHours;
                _logger.LogDebug(
                    "Next AI analysis cycle in {Hours} hour(s)", intervalHours);

                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }
        }
    }

    public class AIAnalysisOptions
    {
        public const string SectionName = "AIAnalysis";
        public int IntervalHours { get; set; } = 24;
    }
}