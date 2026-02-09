using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

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
            var anomaliesList = await _anomalyRepo.ListAllAsync();
            var devices = await _deviceRepo.ListAllAsync();
            var query = anomaliesList.AsEnumerable();

            if (request.IsResolved.HasValue) query = query.Where(a => a.IsResolved == request.IsResolved.Value);
            if (!string.IsNullOrEmpty(request.Severity)) query = query.Where(a => a.Severity == request.Severity);
            if (request.DeviceId.HasValue) query = query.Where(a => a.DeviceId == request.DeviceId.Value);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var statistics = new
            {
                critical = anomaliesList.Count(a => a.Severity?.ToLower() == "critical"),
                unresolved = anomaliesList.Count(a => !a.IsResolved),
                devicesAffected = anomaliesList.Where(a => !a.IsResolved).Select(a => a.DeviceId).Distinct().Count()
            };

            var result = query.OrderByDescending(a => a.DetectedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a =>
                {
                    var device = devices.FirstOrDefault(d => d.Id == a.DeviceId);
                    return new
                    {
                        a.Id,
                        a.DeviceId,
                        a.AnomalyTimestamp,
                        a.ActualValue,
                        a.ExpectedValue,
                        a.Severity,
                        a.Description,
                        a.IsResolved,
                        a.DetectedAt,
                        device = device != null ? new { name = device.Name, zone = device.Zone?.Name ?? "Unknown" } : null,
                        deviationPercent = a.ExpectedValue != 0 ? Math.Round((a.ActualValue - a.ExpectedValue) / a.ExpectedValue * 100, 1) : 0
                    };
                });

            return new ApiResponse(200, "Anomalies retrieved successfully", new
            {
                request.Page,
                request.PageSize,
                totalCount,
                totalPages,
                statistics,
                data = result
            });
        }
    }
}
