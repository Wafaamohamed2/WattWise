using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Interfaces;
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
            var analyses = await _analysisRepo.ListAllAsync();
            var query = analyses.AsEnumerable();

            if (!string.IsNullOrEmpty(request.AnalysisType))
                query = query.Where(a => a.AnalysisType == request.AnalysisType);

            if (request.StartDate.HasValue)
                query = query.Where(a => a.AnalysisDate >= request.StartDate);

            if (request.EndDate.HasValue)
                query = query.Where(a => a.AnalysisDate <= request.EndDate.Value.AddDays(1));

            var total = query.Count();
            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            var data = query.OrderByDescending(a => a.AnalysisDate)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(a => new {
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