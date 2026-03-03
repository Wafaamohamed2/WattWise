using MediatR;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetHourlyConsumptionHandler : IRequestHandler<GetHourlyConsumptionQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        public GetHourlyConsumptionHandler(IGenericRepository<EnergyReading> readingRepo) => _readingRepo = readingRepo;

        public async Task<ApiResponse> Handle(GetHourlyConsumptionQuery request, CancellationToken ct)
        {
            if (!DateTime.TryParse(request.Date, out DateTime targetDate)) targetDate = DateTime.UtcNow.Date;

            var readings = await _readingRepo.ListAsync(new HourlyReadingsSpec(targetDate));

            var hourlyData = Enumerable.Range(0, 24).Select(hour =>
            {
                var hourReadings = readings.Where(r => r.Timestamp.Hour == hour).ToList();
                return new HourlyConsumptionDto
                {
                    Hour = hour,
                    TimeLabel = $"{hour:D2}:00",
                    TotalConsumption = Math.Round(hourReadings.Sum(r => r.PowerConsumptionKW), 2)
                };
            }).ToList();

            return new ApiResponse(200, "Hourly consumption retrieved", hourlyData);
        }
    }
}