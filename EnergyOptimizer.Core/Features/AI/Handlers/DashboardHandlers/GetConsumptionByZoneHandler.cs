using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DashboardHandlers
{
    public class GetConsumptionByZoneHandler : IRequestHandler<GetConsumptionByZoneQuery, ApiResponse>
    {
        private readonly IGenericRepository<Zone> _zoneRepo;
        public GetConsumptionByZoneHandler(IGenericRepository<Zone> zoneRepo) => _zoneRepo = zoneRepo;

        public async Task<ApiResponse> Handle(GetConsumptionByZoneQuery request, CancellationToken ct)
        {

            if (!DateTime.TryParse(request.StartDate, out var start)) start = DateTime.UtcNow.Date;
            if (!DateTime.TryParse(request.EndDate, out var end)) end = DateTime.UtcNow;

            var zones = await _zoneRepo.ListAllAsync();



            return new ApiResponse(200, "Zone consumption statistics retrieved", new { /* البيانات */ });
        }
    }
}
