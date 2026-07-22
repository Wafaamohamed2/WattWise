using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.API.Services
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RefreshTokenCleanupService> _logger;

        public RefreshTokenCleanupService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<RefreshTokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalHours = int.TryParse(_configuration["RefreshToken:CleanupIntervalHours"], out var hours) ? hours : 24;
            var interval = TimeSpan.FromHours(intervalHours);

            _logger.LogInformation("RefreshTokenCleanupService started. Cleanup interval: {Hours} hours.", intervalHours);

            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanUpTokensAsync(stoppingToken);

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task CleanUpTokensAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EnergyDbContext>();

                var retentionDays = int.TryParse(_configuration["RefreshToken:RetentionDaysAfterExpiry"], out var days) ? days : 7;
                var thresholdDate = DateTime.UtcNow.AddDays(-retentionDays);

                var deletedTokensCount = await context.RefreshTokens
                    .Where(t => t.ExpiresOn < thresholdDate || (t.RevokedOn != null && t.RevokedOn < thresholdDate))
                    .ExecuteDeleteAsync(stoppingToken);

                if (deletedTokensCount > 0)
                {
                    _logger.LogInformation("RefreshTokenCleanupService: Successfully deleted {Count} expired/revoked refresh tokens older than {Days} days.", deletedTokensCount, retentionDays);
                }
                else
                {
                    _logger.LogInformation("RefreshTokenCleanupService: No expired or revoked refresh tokens to clean up.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshTokenCleanupService: An error occurred while cleaning up refresh tokens.");
            }
        }
    }
}
