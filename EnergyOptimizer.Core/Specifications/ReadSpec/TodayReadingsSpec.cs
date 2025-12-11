using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class TodayReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public TodayReadingsSpec()
           : base(r => r.Timestamp >= DateTime.UtcNow.Date)
        {
            AddInclude(r => r.Device);
            ApplyOrderByDescending(r => r.Timestamp);
        }
    }
}
