using EnergyOptimizer.Core.Entities.AI_Analysis;


namespace EnergyOptimizer.Core.Specifications.AnomaliesSpec
{
    public class AnomaliesCountSpec : BaseSpecifcation<DetectedAnomaly>
    {
        public AnomaliesCountSpec(bool? isResolved, string? severity, int? deviceId)
            : base(a =>
                (!isResolved.HasValue || a.IsResolved == isResolved.Value) &&
                (string.IsNullOrEmpty(severity) || a.Severity == severity) &&
                (!deviceId.HasValue || a.DeviceId == deviceId.Value))
        {
        }
    }
}
