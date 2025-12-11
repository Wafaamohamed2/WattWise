using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class DeviceActivitySpec : BaseSpecifcation<EnergyReading>
    {
        public DeviceActivitySpec(int minutesAgo = 5)
            : base(r => r.Timestamp >= DateTime.UtcNow.AddMinutes(-minutesAgo))
        {
            AddInclude(r => r.Device);
            ApplyOrderByDescending(r => r.Timestamp);
            ApplyDistinct();  
        }
    }
}
