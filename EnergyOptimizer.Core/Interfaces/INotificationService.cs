using EnergyOptimizer.Core.DTOs.AlertsDTOs;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface INotificationService
    {
        Task BroadcastAlertAsync(AlertDto alert, CancellationToken cancellationToken = default);
        Task SendAlertToUserAsync(string userId, AlertDto alert, CancellationToken cancellationToken = default);
        Task SendUnreadCountUpdateAsync(string userId, int unreadCount, CancellationToken cancellationToken = default);
        Task BroadcastSystemMessageAsync(string title, string message, string severity = "info", CancellationToken cancellationToken = default);
    }
}
