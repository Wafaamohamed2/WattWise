using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DeviceWithDetailsSpec : BaseSpecifcation<Device>
    {
        public DeviceWithDetailsSpec(int deviceId)
            : base(d => d.Id == deviceId)
        {
            AddInclude(d => d.Name);
            AddInclude(d => d.Zone);
            AddInclude(d => d.EnergyReadings);
        }
    }
}
