using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers
{
    public class AskAIQuestionHandler : IRequestHandler<AskAIQuestionQuery, ApiResponse>
    {
        private readonly IPatternDetectionService _patternService;

        public AskAIQuestionHandler(IPatternDetectionService patternService)
        {
            _patternService = patternService;
        }

        public async Task<ApiResponse> Handle(AskAIQuestionQuery request, CancellationToken ct)
        {
            var context = request.Context ?? "You are an energy optimization assistant helping users reduce electricity consumption.";

            var answer = await _patternService.AskQuestion(request.Question, context);

            return new ApiResponse(200, "Answer retrieved successfully", new { answer });
        }
    }
}