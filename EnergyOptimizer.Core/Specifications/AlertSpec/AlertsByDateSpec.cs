using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertsByDateSpec : BaseSpecifcation<Alert>
    {
        public AlertsByDateSpec(DateTime startDate)
            : base(a => a.CreatedAt >= startDate)
        {
            ApplyOrderByDescending(a => a.CreatedAt);
        }
    }
}
