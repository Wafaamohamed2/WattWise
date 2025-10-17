using Microsoft.AspNetCore.SignalR;

namespace EnergyOptimizer.API.Hubs
{
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
                message = "Latest data request received",
                timestamp = DateTime.UtcNow
            });
        }

        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

    }
}
