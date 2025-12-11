using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class PaginatedActiveDevicesSpec : BaseSpecifcation<Device>
    {
        public PaginatedActiveDevicesSpec(int pageIndex, int pageSize)
            : base(d => d.IsActive)
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
            ApplyPaging(pageIndex * pageSize, pageSize);
        }
    }
}
