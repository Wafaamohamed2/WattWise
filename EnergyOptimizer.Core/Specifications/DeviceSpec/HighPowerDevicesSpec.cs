using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class HighPowerDevicesSpec : BaseSpecifcation<Device>
    {
        public HighPowerDevicesSpec(decimal minPowerKW =(decimal) 1.5 )
           : base(d => d.RatedPowerKW >= minPowerKW && d.IsActive)
        {
            AddInclude(d => d.Zone);
            ApplyOrderByDescending(d => d.RatedPowerKW);
        }
    }
}
