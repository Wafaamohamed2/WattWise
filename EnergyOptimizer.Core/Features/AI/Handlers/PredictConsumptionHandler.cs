using MediatR;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers
{
    public class PredictConsumptionHandler : IRequestHandler<PredictConsumptionQuery, ApiResponse>
    {
        private readonly IPatternDetectionService _patternService;

        public PredictConsumptionHandler(IPatternDetectionService patternService)
        {
            _patternService = patternService;
        }

        public async Task<ApiResponse> Handle(PredictConsumptionQuery request, CancellationToken ct)
        {
            var result = await _patternService.PredictConsumption(request.Days);

            if (result.ConfidenceScore == 0)
                throw new BadRequestException(result.Explanation);

            return new ApiResponse(200, "Consumption prediction generated successfully", new
            {
                predictionDate = result.PredictionDate.ToString("yyyy-MM-dd"),
                predictedConsumptionKWh = result.PredictedConsumptionKWh,
                confidenceScore = result.ConfidenceScore,
                explanation = result.Explanation
            });
        }
    }
}