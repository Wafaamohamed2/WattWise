using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetTopConsumersHandler : IRequestHandler<GetTopConsumersQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetTopConsumersHandler(IGenericRepository<EnergyReading> readingRepo, ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetTopConsumersQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var readings = await _readingRepo.ListAsync(new LatestReadingsSpec(userId, 500));

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