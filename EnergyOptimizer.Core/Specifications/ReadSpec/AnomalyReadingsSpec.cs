using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class AnomalyReadingsSpec: BaseSpecifcation<EnergyReading>
    {
        public AnomalyReadingsSpec(
           int deviceId,
           double avgConsumption,
           double stdDeviation,
           double thresholdMultiplier = 2.0)
           : base(r => r.DeviceId == deviceId &&
                       (r.PowerConsumptionKW > avgConsumption + (stdDeviation * thresholdMultiplier) ||
                        r.PowerConsumptionKW < Math.Max(0, avgConsumption - (stdDeviation * thresholdMultiplier))))
        {
            AddInclude(r => r.Device);
            ApplyOrderByDescending(r => r.Timestamp);
        }
    }
}
