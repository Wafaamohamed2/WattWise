using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Service.Services.Abstract;

namespace EnergyOptimizer.Core.Features.AI.Handlers
{
    public class RunGlobalAnalysisHandler : IRequestHandler<RunGlobalAnalysisCommand, ApiResponse>
    {
        private readonly IAIAnalysisService _aiService;

        public RunGlobalAnalysisHandler(IAIAnalysisService aiService)
        {
            _aiService = aiService;
        }

        public async Task<ApiResponse> Handle(RunGlobalAnalysisCommand request, CancellationToken cancellationToken)
        {
            await _aiService.RunGlobalAnalysisAsync(cancellationToken);

            return new ApiResponse(200, "AI Global Analysis triggered via Mediator successfully");
        }
    }
}