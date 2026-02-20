using MediatR;
using System.Text.Json;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions; 
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers
{
    public class GenerateRecommendationsHandler : IRequestHandler<GenerateRecommendationsCommand, ApiResponse>
    {
        private readonly IPatternDetectionService _patternService;
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;

        public GenerateRecommendationsHandler(
            IPatternDetectionService patternService,
            IGenericRepository<EnergyAnalysis> analysisRepo,
            IGenericRepository<EnergyRecommendation> recommendationRepo)
        {
            _patternService = patternService;
            _analysisRepo = analysisRepo;
            _recommendationRepo = recommendationRepo;
        }

        public async Task<ApiResponse> Handle(GenerateRecommendationsCommand request, CancellationToken ct)
        {
            var start = request.StartDate ?? DateTime.UtcNow.AddDays(-30);
            var end = request.EndDate ?? DateTime.UtcNow;

            if (start >= end)
                throw new BadRequestException("Start date must be before end date");

            var result = await _patternService.GenerateRecommendations(start, end);

            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = "Recommendations",
                Summary = $"Generated {result.Recommendations.Count} recommendations",
                FullResponse = JsonSerializer.Serialize(result),
                PeriodStart = start,
                PeriodEnd = end,
                TotalConsumptionKWh = 0,
                DevicesAnalyzed = 0
            };

            await _analysisRepo.AddAsync(analysis);
            await _analysisRepo.SaveChangesAsync();

            foreach (var rec in result.Recommendations)
            {
                await _recommendationRepo.AddAsync(new EnergyRecommendation
                {
                    Title = rec.Title,
                    Description = rec.Description,
                    Category = rec.Category,
                    Priority = rec.Priority,
                    EstimatedSavingsKWh = rec.PotentialSavingsKWh,
                    AnalysisId = analysis.Id
                });
            }

            await _recommendationRepo.SaveChangesAsync();

            return new ApiResponse(200, "Energy saving recommendations generated and saved successfully", new
            {
                count = result.Recommendations.Count,
                analysisId = analysis.Id
            });
        }
    }
}