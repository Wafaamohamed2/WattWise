using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ReadingsByDeviceAndDateSpec : BaseSpecifcation<EnergyReading>
    {
        public ReadingsByDeviceAndDateSpec(
           int deviceId,
           DateTime startDate,
           DateTime endDate)
           : base(r => r.DeviceId == deviceId &&
                       r.Timestamp >= startDate &&
                       r.Timestamp <= endDate)
        {
            AddInclude(r => r.Device);
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
