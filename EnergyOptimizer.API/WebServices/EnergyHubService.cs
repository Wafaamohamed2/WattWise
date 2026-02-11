using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EnergyOptimizer.API.WebServices
{
    public class EnergyHubService : IEnergyHubService
    {
        private readonly IHubContext<EnergyHub> _hubContext;

        public EnergyHubService(IHubContext<EnergyHub> hubContext)
        {
            _hubContext = hubContext;
        }
        public async Task SendAlertNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveAlert", message);
        }
        public async Task NotifyDeviceStatusChanged(int deviceId, bool isActive)
        {
            await _hubContext.Clients.All.SendAsync("DeviceStatusUpdated", new
            {
                DeviceId = deviceId,
                IsActive = isActive
            });
        }

        public async Task NotifyNewReading(object readingData)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveReading", readingData);
        }
        
    }
}
