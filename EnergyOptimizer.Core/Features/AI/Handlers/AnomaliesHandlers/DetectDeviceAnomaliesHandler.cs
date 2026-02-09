using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnomaliesHandlers
{
    public class DetectDeviceAnomaliesHandler : IRequestHandler<DetectDeviceAnomaliesCommand, ApiResponse>
    {
        private readonly IPatternDetectionService _patternService;

        public DetectDeviceAnomaliesHandler(IPatternDetectionService patternService)
        {
            _patternService = patternService;
        }

        public async Task<ApiResponse> Handle(DetectDeviceAnomaliesCommand request, CancellationToken cancellationToken)
        {
            var result = await _patternService.DetectDeviceAnomalies(request.DeviceId, request.Days);

            return new ApiResponse(200, "Anomaly detection completed successfully", new
            {
                request.DeviceId,
                daysAnalyzed = request.Days,
                hasAnomalies = result.HasAnomalies,
                anomaliesCount = result.Anomalies.Count,
                anomalies = result.Anomalies,
                analysis = result.Analysis
            });
        }
    }
}
