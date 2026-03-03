using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AnalysisSpec;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AnalyzeHandlers
{
    public class GetAnalysisHistoryHandler : IRequestHandler<GetAnalysisHistoryQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;

        public GetAnalysisHistoryHandler(IGenericRepository<EnergyAnalysis> analysisRepo)
        {
            _analysisRepo = analysisRepo;
        }

        public async Task<ApiResponse> Handle(GetAnalysisHistoryQuery request, CancellationToken ct)
        {
            var countSpec = new AnalysisHistoryCountSpec(
                request.AnalysisType, request.StartDate, request.EndDate);

            var total = await _analysisRepo.CountAsync(countSpec);
            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            var dataSpec = new AnalysisHistorySpec(
                request.AnalysisType, request.StartDate, request.EndDate,
                request.Page, request.PageSize);

            var analyses = await _analysisRepo.ListAsync(dataSpec);

            var data = analyses.Select(a => new
            {
                a.Id,
                a.AnalysisType,
                a.AnalysisDate,
                a.Summary,
                a.TotalConsumptionKWh,
                a.DevicesAnalyzed
            });

            return new ApiResponse(200, "Analysis history retrieved", new
            {
                request.Page,
                request.PageSize,
                totalPages,
                count = total,
                data
            });
        }
    }
}