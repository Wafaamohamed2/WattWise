using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class CountReadingsByDeviceSpec: BaseSpecifcation<EnergyReading>
    {
        public CountReadingsByDeviceSpec(int deviceId, DateTime? since = null)
            : base(r => r.DeviceId == deviceId &&
                        (!since.HasValue || r.Timestamp >= since.Value))
        {
        }
    }
}
