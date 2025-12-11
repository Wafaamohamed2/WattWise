using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class CountActiveDevicesSpec : BaseSpecifcation<Device>
    {
        public CountActiveDevicesSpec() : base(d => d.IsActive)
        {
        }

    }
}
