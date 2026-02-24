using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ReadingsByDateRangeSpec : BaseSpecifcation<EnergyReading>
    {
        public ReadingsByDateRangeSpec(DateTime start, DateTime end)
            : base(r => r.Timestamp >= start && r.Timestamp <= end)
        {
            AddInclude(r => r.Device);
            AddInclude(r => r.Device.Zone);
            ApplyOrderByDescending(r => r.Timestamp);
        }
    }
}
