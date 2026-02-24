using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetTopConsumersHandler : IRequestHandler<GetTopConsumersQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;

        public GetTopConsumersHandler(IGenericRepository<EnergyReading> readingRepo)
        {
            _readingRepo = readingRepo;
        }

        public async Task<ApiResponse> Handle(GetTopConsumersQuery request, CancellationToken ct)
        {
            var readings = await _readingRepo.ListAsync(new LatestReadingsSpec(500));

            var topConsumers = readings
                .GroupBy(r => new { r.DeviceId, DeviceName = r.Device?.Name, ZoneName = r.Device?.Zone?.Name })
                .Select(g => new
                {
                    deviceId = g.Key.DeviceId,
                    deviceName = g.Key.DeviceName ?? "Unknown",
                    zoneName = g.Key.ZoneName ?? "Unknown",
                    totalConsumption = Math.Round(g.Sum(r => r.PowerConsumptionKW), 3)
                })
                .OrderByDescending(x => x.totalConsumption)
                .Take(request.Count)
                .ToList();

            return new ApiResponse(200, "Top consumers retrieved", topConsumers);
        }
    }
}