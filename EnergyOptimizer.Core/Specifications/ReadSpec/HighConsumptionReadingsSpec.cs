using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class HighConsumptionReadingsSpec :BaseSpecifcation<EnergyReading>
    {
        public HighConsumptionReadingsSpec(decimal threshold, DateTime since)
          : base(r => r.PowerConsumptionKW >= threshold &&
                      r.Timestamp >= since)
        {
            AddInclude(r => r.Device);
            AddInclude("Device.Zone");
            ApplyOrderByDescending(r => r.PowerConsumptionKW);
        }
    }
}
