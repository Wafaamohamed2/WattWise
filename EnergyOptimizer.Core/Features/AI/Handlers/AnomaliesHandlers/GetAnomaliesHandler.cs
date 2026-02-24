using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

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
            var anomalies = await _anomalyRepo.ListAllAsync();

            var query = anomalies.AsQueryable();

            if (request.IsResolved.HasValue)
                query = query.Where(a => a.IsResolved == request.IsResolved.Value);

            if (!string.IsNullOrEmpty(request.Severity))
                query = query.Where(a => a.Severity == request.Severity);

            var totalItems = query.Count();
            var items = query
                .OrderByDescending(a => a.DetectedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var responseData = new
            {
                items = items,
                totalItems = totalItems,
                page = request.Page,
                totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize)
            };

            return new ApiResponse(200, "Anomalies retrieved", responseData);
        }
    }
}