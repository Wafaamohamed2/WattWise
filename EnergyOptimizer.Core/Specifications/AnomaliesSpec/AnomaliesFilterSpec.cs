using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Core.Specifications.AnomaliesSpec
{
    public class AnomaliesFilterSpec : BaseSpecifcation<DetectedAnomaly>
    {
        public AnomaliesFilterSpec(
            bool? isResolved,
            string? severity,
            int? deviceId,
            int page,
            int pageSize)
            : base(a =>
                (!isResolved.HasValue || a.IsResolved == isResolved.Value) &&
                (string.IsNullOrEmpty(severity) || a.Severity == severity) &&
                (!deviceId.HasValue || a.DeviceId == deviceId.Value))
        {
            AddInclude(a => a.Device);
            ApplyOrderByDescending(a => a.DetectedAt);
            ApplyPaging((page - 1) * pageSize, pageSize);
        }
    }

}
