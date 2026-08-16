using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.Dashboard.Queries;
using EnergyOptimizer.Core.Contracts;
using Microsoft.EntityFrameworkCore;

namespace EnergyOptimizer.Core.Features.Dashboard.Handlers
{
    public class GetConsumptionTrendHandler : IRequestHandler<GetConsumptionTrendQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetConsumptionTrendHandler(IGenericRepository<EnergyReading> readingRepo, ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetConsumptionTrendQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            int hours = request.Hours > 0 ? request.Hours : 24;
            var start = DateTime.UtcNow.AddHours(-hours);

            var trendData = await _readingRepo.GetQueryable()
                .Where(r => r.Timestamp >= start &&
                            r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null &&
                            r.Device.Zone.Building.UserId == userId &&
                            (!request.BuildingId.HasValue || r.Device.Zone.BuildingId == request.BuildingId.Value))
                .GroupBy(r => new { r.Timestamp.Date, r.Timestamp.Hour })
                .Select(g => new
                {
                    timestamp = g.Key.Date.AddHours(g.Key.Hour),
                    totalConsumptionKW = Math.Round(g.Sum(r => (decimal?)r.PowerConsumptionKW) ?? 0m, 3),
                    avgTemperature = Math.Round(g.Average(r => (decimal?)r.Temperature) ?? 0m, 1)
                })
                .OrderBy(x => x.timestamp)
                .ToListAsync(ct);

            return new ApiResponse(200, "Consumption trend retrieved successfully", trendData);
        }
    }
}
