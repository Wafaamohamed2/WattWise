using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Specifications.AnomaliesSpec;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers
{
    public class GetAnomaliesHandler : IRequestHandler<GetAnomaliesQuery, ApiResponse>
    {
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;
        private readonly IGenericRepository<Device> _deviceRepo;

        public GetAnomaliesHandler(IGenericRepository<DetectedAnomaly> anomalyRepo, IGenericRepository<Device> deviceRepo)
        {
            _anomalyRepo = anomalyRepo;
            _deviceRepo = deviceRepo;
        }

        public async Task<ApiResponse> Handle(GetAnomaliesQuery request, CancellationToken ct)
        {
            var countSpec = new AnomaliesCountSpec(request.IsResolved, request.Severity, request.DeviceId);
            var total = await _anomalyRepo.CountAsync(countSpec);

            var dataSpec = new AnomaliesFilterSpec(
                request.IsResolved, request.Severity, request.DeviceId,
                request.Page, request.PageSize);

            var items = await _anomalyRepo.ListAsync(dataSpec);

            return new ApiResponse(200, "Anomalies retrieved", new
            {
                items,
                totalItems = total,
                page = request.Page,
                pageSize = request.PageSize,
                totalPages = (int)Math.Ceiling(total / (double)request.PageSize)
            });
        }
    }
}