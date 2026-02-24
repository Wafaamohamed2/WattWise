using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers
{
    public class GetLatestReadingsHandler : IRequestHandler<GetLatestReadingsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;

        public GetLatestReadingsHandler(IGenericRepository<EnergyReading> readingRepo)
        {
            _readingRepo = readingRepo;
        }

        public async Task<ApiResponse> Handle(GetLatestReadingsQuery request, CancellationToken ct)
        {
            IReadOnlyList<EnergyReading> readings;

            if (!string.IsNullOrEmpty(request.StartDate) && DateTime.TryParse(request.StartDate, out var start)
                && !string.IsNullOrEmpty(request.EndDate) && DateTime.TryParse(request.EndDate, out var end))
            {
                readings = await _readingRepo.ListAsync(new ReadingsByDateRangeSpec(start, end));
            }
            else
            {
                readings = await _readingRepo.ListAsync(new LatestReadingsSpec(request.Limit));
            }

            var result = readings.Select(r => new
            {
                r.Id,
                r.PowerConsumptionKW,
                r.Voltage,
                r.Current,
                r.Temperature,
                r.Timestamp,
                deviceName = r.Device?.Name ?? "Unknown",
                deviceType = r.Device?.Type.ToString() ?? "Unknown",
                zoneName = r.Device?.Zone?.Name ?? "Unknown",
                zoneId = r.Device?.ZoneId,
                deviceId = r.DeviceId
            });

            return new ApiResponse(200, "Latest readings retrieved", result);
        }
    }
}