using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers
{
    public class CreateReadingHandler : IRequestHandler<CreateReadingCommand, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IEnergyHubService _hubService;

        public CreateReadingHandler(IGenericRepository<EnergyReading> readingRepo, IEnergyHubService hubService)
        {
            _readingRepo = readingRepo;
            _hubService = hubService;
        }

        public async Task<ApiResponse> Handle(CreateReadingCommand request, CancellationToken ct)
        {
            var reading = new EnergyReading
            {
                DeviceId = request.Dto.DeviceId,
                PowerConsumptionKW = request.Dto.PowerConsumptionKW,
                Voltage = request.Dto.Voltage,
                Current = request.Dto.Current,
                Temperature = request.Dto.Temperature,
                Timestamp = DateTime.UtcNow
            };

            _readingRepo.Add(reading);
            await _readingRepo.SaveChangesAsync();

            await _hubService.NotifyNewReading(new
            {
                reading.DeviceId,
                reading.PowerConsumptionKW,
                reading.Timestamp
            });

            return new ApiResponse(201, "Reading recorded and broadcasted");
        }
    }
}