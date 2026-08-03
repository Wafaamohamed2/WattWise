using MediatR;
using MassTransit;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Features.Readings.Commands;

namespace EnergyOptimizer.Core.Features.Readings.Handlers
{
    public class CreateReadingHandler : IRequestHandler<CreateReadingCommand, ApiResponse>
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public CreateReadingHandler(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task<ApiResponse> Handle(CreateReadingCommand request, CancellationToken ct)
        {
            var readingEvent = new EnergyReadingReceivedEvent(
                request.Dto.DeviceId,
                request.Dto.PowerConsumptionKW,
                request.Dto.Voltage,
                request.Dto.Current,
                request.Dto.Temperature,
                DateTime.UtcNow
            );

            await _publishEndpoint.Publish(readingEvent, ct);

            return new ApiResponse(202, "Reading queued successfully for background processing.");
        }
    }
}