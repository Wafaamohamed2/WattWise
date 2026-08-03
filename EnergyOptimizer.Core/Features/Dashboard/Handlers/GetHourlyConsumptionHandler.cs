using MediatR;
using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.Dashboard.Queries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Dashboard.Handlers
{
    public class GetHourlyConsumptionHandler : IRequestHandler<GetHourlyConsumptionQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly ICurrentUserService _currentUser;

        public GetHourlyConsumptionHandler(IGenericRepository<EnergyReading> readingRepo, ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetHourlyConsumptionQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            if (!DateTime.TryParse(request.Date, out DateTime targetDate)) targetDate = DateTime.UtcNow.Date;

            var nextDay = targetDate.Date.AddDays(1);

            var dbHourlyStats = await _readingRepo.GetQueryable()
                .Where(r => r.Timestamp >= targetDate.Date && r.Timestamp < nextDay &&
                            r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null &&
                            r.Device.Zone.Building.UserId == userId)
                .GroupBy(r => r.Timestamp.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    TotalConsumption = g.Sum(r => r.PowerConsumptionKW)
                })
                .ToDictionaryAsync(x => x.Hour, x => x.TotalConsumption, ct);

            var hourlyData = Enumerable.Range(0, 24).Select(hour => new HourlyConsumptionDto
            {
                Hour = hour,
                TimeLabel = $"{hour:D2}:00",
                TotalConsumption = Math.Round(dbHourlyStats.TryGetValue(hour, out var val) ? val : 0m, 2)
            }).ToList();

            return new ApiResponse(200, "Hourly consumption retrieved", hourlyData);
        }
    }
}