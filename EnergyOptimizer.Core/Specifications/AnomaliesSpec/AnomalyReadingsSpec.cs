using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AnomaliesSpec
{
    public class AnomalyReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public AnomalyReadingsSpec(
           int deviceId,
           decimal avgConsumption,
           decimal stdDeviation,
           decimal thresholdMultiplier = 2)
           : base(r => r.DeviceId == deviceId &&
                       (r.PowerConsumptionKW > avgConsumption + stdDeviation * thresholdMultiplier ||
                        r.PowerConsumptionKW < Math.Max(0, avgConsumption - stdDeviation * thresholdMultiplier)))
        {
            AddInclude(r => r.Device);
            ApplyOrderByDescending(r => r.Timestamp);
        }
    }
}
