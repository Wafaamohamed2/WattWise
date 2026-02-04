using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DeviceFilterSpec : BaseSpecifcation<Device>
    {
        public DeviceFilterSpec(
              bool? isActive = null,
              int? zoneId = null,
              DeviceType? deviceType = null,
              decimal? minPower = null,
              decimal? maxPower = null,
              int pageIndex = 0,
              int pageSize = 20)
              : base(d =>
                  (!isActive.HasValue || d.IsActive == isActive.Value) &&
                  (!zoneId.HasValue || d.ZoneId == zoneId.Value) &&
                  (!deviceType.HasValue || d.Type == deviceType.Value) &&
                  (!minPower.HasValue || d.RatedPowerKW >= minPower.Value) &&
                  (!maxPower.HasValue || d.RatedPowerKW <= maxPower.Value)
              )
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);

            if (pageSize > 0)
            {
                ApplyPaging(pageIndex * pageSize, pageSize);
            }
        }
    }
}
