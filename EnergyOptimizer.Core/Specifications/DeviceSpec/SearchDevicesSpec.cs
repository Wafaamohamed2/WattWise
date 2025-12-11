using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class SearchDevicesSpec : BaseSpecifcation<Device>
    {
        public SearchDevicesSpec(string searchTerm)
         : base(d => d.Name.Contains(searchTerm))
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
        }
    }
}
