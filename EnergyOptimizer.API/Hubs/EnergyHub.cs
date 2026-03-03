using Microsoft.AspNet.SignalR;
using Microsoft.AspNetCore.SignalR;
using Hub = Microsoft.AspNetCore.SignalR.Hub;

namespace EnergyOptimizer.API.Hubs
{
    [Authorize]
    public class EnergyHub: Hub
    {

        private readonly ILogger<EnergyHub> _logger;

        public EnergyHub(ILogger<EnergyHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("Client connected: {ConnectionId}", connectionId);

            await Clients.Caller.SendAsync("Connected", new
            {
                connectionId,
                message = "Successfully connected to Energy Hub",
                timestamp = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;

            if (exception != null)
            {
                _logger.LogError(exception, "Client disconnected with error: {ConnectionId}", connectionId);
            }
            else
            {
                _logger.LogInformation("Client disconnected: {ConnectionId}", connectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
        public async Task RequestLatestData()
        {
            _logger.LogInformation("Client {ConnectionId} requested latest data", Context.ConnectionId);

            await Clients.Caller.SendAsync("DataRequested", new
            {
                message = "Subscribed – next reading cycle will be pushed to you automatically",
                timestamp = DateTime.UtcNow
            });
        }


        public async Task JoinZone(string zoneName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Zone_{zoneName}");
            _logger.LogInformation("Client {ConnectionId} joined zone group: {Zone}", Context.ConnectionId, zoneName);
        }

        public async Task LeaveZone(string zoneName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Zone_{zoneName}");
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

    }
}
