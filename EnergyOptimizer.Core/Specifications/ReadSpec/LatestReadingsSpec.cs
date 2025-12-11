using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class LatestReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public LatestReadingsSpec(int limit = 50)
        {
            AddInclude(r => r.Device);
            AddInclude("Device.Zone");
            ApplyOrderByDescending(r => r.Timestamp);
            ApplyPaging(0, limit);
        }
    }
}
