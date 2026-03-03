using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Core.Specifications.AnalysisSpec
{
    public class AnalysisHistorySpec : BaseSpecifcation<EnergyAnalysis>
    {
        public AnalysisHistorySpec(
            string? analysisType,
            DateTime? startDate,
            DateTime? endDate,
            int page,
            int pageSize)
            : base(a =>
                (string.IsNullOrEmpty(analysisType) || a.AnalysisType == analysisType) &&
                (!startDate.HasValue || a.AnalysisDate >= startDate.Value) &&
                (!endDate.HasValue || a.AnalysisDate <= endDate.Value.AddDays(1)))
        {
            ApplyOrderByDescending(a => a.AnalysisDate);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }
    public class AnalysisHistoryCountSpec : BaseSpecifcation<EnergyAnalysis>
    {
        public AnalysisHistoryCountSpec(string? analysisType, DateTime? startDate, DateTime? endDate)
            : base(a =>
                (string.IsNullOrEmpty(analysisType) || a.AnalysisType == analysisType) &&
                (!startDate.HasValue || a.AnalysisDate >= startDate.Value) &&
                (!endDate.HasValue || a.AnalysisDate <= endDate.Value.AddDays(1)))
        {
        }
    }

}
