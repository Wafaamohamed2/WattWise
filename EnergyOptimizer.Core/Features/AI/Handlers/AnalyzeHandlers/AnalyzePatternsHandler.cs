using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions; 
using MediatR;
using System.Text.Json;
using EnergyOptimizer.Core.Features.AI.Commands;


namespace EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers
{
    public class AnalyzePatternsHandler : IRequestHandler<AnalyzePatternsQuery, ApiResponse>
    {
        private readonly IPatternDetectionService _patternService;
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;

        public AnalyzePatternsHandler(IPatternDetectionService patternService, IGenericRepository<EnergyAnalysis> analysisRepo)
        {
            _patternService = patternService;
            _analysisRepo = analysisRepo;
        }

        public async Task<ApiResponse> Handle(AnalyzePatternsQuery request, CancellationToken ct)
        {
            var start = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var end = request.EndDate ?? DateTime.UtcNow;

            if (start >= end)
                throw new BadRequestException("Start date must be before end date");

            var result = await _patternService.AnalyzeConsumptionPatterns(start, end);

            if (!result.Success)
                throw new BadRequestException(result.ErrorMessage);

            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = "Pattern",
                PeriodStart = start,
                PeriodEnd = end,
                Summary = result.Summary,
                FullResponse = JsonSerializer.Serialize(result)
            };

            await _analysisRepo.AddAsync(analysis);
            await _analysisRepo.SaveChangesAsync();

            return new ApiResponse(200, "Analysis completed successfully", result);
        }
    }
}