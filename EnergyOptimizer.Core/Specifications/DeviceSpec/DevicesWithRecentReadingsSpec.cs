using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DevicesWithRecentReadingsSpec : BaseSpecifcation<Device>
    {
        public DevicesWithRecentReadingsSpec(DateTime since)
           : base(d => d.IsActive &&
                       d.EnergyReadings.Any(r => r.Timestamp >= since))
        {
            AddInclude(d => d.Zone);
            AddInclude(d => d.EnergyReadings.Where(r => r.Timestamp >= since));
            ApplyOrderBy(d => d.Name);
        }
    }
}
