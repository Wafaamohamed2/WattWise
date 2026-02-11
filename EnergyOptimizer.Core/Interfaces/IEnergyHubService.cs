

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IEnergyHubService
    {
        Task SendAlertNotification(string message);
        Task NotifyDeviceStatusChanged(int deviceId, bool isActive);
        Task NotifyNewReading(object readingData);
    }
}
