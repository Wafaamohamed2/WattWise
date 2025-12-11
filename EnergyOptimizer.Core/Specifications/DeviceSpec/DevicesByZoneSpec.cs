using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DevicesByZoneSpec : BaseSpecifcation<Device>
    {
        public DevicesByZoneSpec(int zoneId)
            : base(d => d.ZoneId == zoneId)
        {
            AddInclude(d => d.Zone);
            ApplyOrderByDescending(d => d.RatedPowerKW);  // Highest power first

        }
    }
}
