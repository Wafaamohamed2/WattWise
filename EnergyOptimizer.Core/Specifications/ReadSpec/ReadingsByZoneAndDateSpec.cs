using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ReadingsByZoneAndDateSpec : BaseSpecifcation<EnergyReading>
    {
        public ReadingsByZoneAndDateSpec(
           int zoneId,
           DateTime startDate,
           DateTime endDate)
           : base(r => r.Device.ZoneId == zoneId &&
                       r.Timestamp >= startDate &&
                       r.Timestamp <= endDate)
        {
            AddInclude(r => r.Device);
            AddInclude("Device.Zone");
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
