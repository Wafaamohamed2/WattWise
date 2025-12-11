using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class HourlyReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public HourlyReadingsSpec(DateTime date)
           : base(r => r.Timestamp >= date.Date &&
                       r.Timestamp < date.Date.AddDays(1))
        {
            AddInclude(r => r.Device);
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
