using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Service.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EnergyOptimizer.Service.Services
{
    public class SignalRNotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<SignalRNotificationService> _logger;

        public SignalRNotificationService(IHubContext<NotificationHub> hubContext, ILogger<SignalRNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task BroadcastAlertAsync(AlertDto alert, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveAlert", alert, cancellationToken);
                _logger.LogInformation("Broadcasted real-time alert via SignalR: AlertId={AlertId}, Device={DeviceName}", alert.Id, alert.DeviceName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast real-time alert via SignalR: AlertId={AlertId}", alert.Id);
            }
        }

        public async Task SendAlertToUserAsync(string userId, AlertDto alert, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveAlert", alert, cancellationToken);
                _logger.LogInformation("Sent real-time alert to User {UserId} via SignalR: AlertId={AlertId}", userId, alert.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send real-time alert to User {UserId} via SignalR", userId);
            }
        }

        public async Task SendUnreadCountUpdateAsync(string userId, int unreadCount, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.Group($"User_{userId}").SendAsync("UpdateUnreadCount", unreadCount, cancellationToken);
                _logger.LogInformation("Sent unread alert count update to User {UserId}: Count={Count}", userId, unreadCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unread count update to User {UserId} via SignalR", userId);
            }
        }

        public async Task BroadcastSystemMessageAsync(string title, string message, string severity = "info", CancellationToken cancellationToken = default)
        {
            try
            {
                var systemMsg = new { title, message, severity, timestamp = DateTime.UtcNow };
                await _hubContext.Clients.All.SendAsync("ReceiveSystemMessage", systemMsg, cancellationToken);
                _logger.LogInformation("Broadcasted system message via SignalR: Title={Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to broadcast system message via SignalR");
            }
        }
    }
}
