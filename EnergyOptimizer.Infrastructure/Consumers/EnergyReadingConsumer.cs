using MassTransit;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;

namespace EnergyOptimizer.Infrastructure.Consumers
{
    public class EnergyReadingConsumer : IConsumer<EnergyReadingReceivedEvent>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepository;
        private readonly IEnergyHubService _hubService;

        public EnergyReadingConsumer(IGenericRepository<EnergyReading> readingRepository, IEnergyHubService hubService)
        {
            _readingRepository = readingRepository;
            _hubService = hubService;
        }

        public async Task Consume(ConsumeContext<EnergyReadingReceivedEvent> context)
        {
            var message = context.Message;
            
            var reading = new EnergyReading
            {
                DeviceId = message.DeviceId,
                PowerConsumptionKW = message.PowerConsumptionKW,
                Voltage = message.Voltage,
                Current = message.Current,
                Temperature = message.Temperature,
                Timestamp = message.Timestamp
            };

            _readingRepository.Add(reading);
            await _readingRepository.SaveChangesAsync();

            // Broadcast reading in real-time via SignalR
            await _hubService.NotifyNewReading(new
            {
                reading.DeviceId,
                reading.PowerConsumptionKW,
                reading.Timestamp
            });
        }
    }
}
