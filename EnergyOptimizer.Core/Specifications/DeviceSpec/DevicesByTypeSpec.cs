using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DevicesByTypeSpec : BaseSpecifcation<Device>
    {
        public DevicesByTypeSpec(DeviceType type, bool activeOnly = false)
           : base(d => d.Type == type && (!activeOnly || d.IsActive))
        {
            AddInclude(d => d.Zone);
        }
    }
}
