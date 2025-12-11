using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class PaginatedReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public PaginatedReadingsSpec(
            DateTime startDate,
            DateTime endDate,
            int? deviceId = null,
            int? zoneId = null,
            double? minPower = null,
            double? maxPower = null,
            int pageIndex = 0,
            int pageSize = 50)
            : base(r =>
                // Mandatory Date Range
                r.Timestamp >= startDate && r.Timestamp <= endDate &&

                // Optional Filters
                (!deviceId.HasValue || r.DeviceId == deviceId.Value) &&
                (!zoneId.HasValue || r.Device.ZoneId == zoneId.Value) &&
                (!minPower.HasValue || r.PowerConsumptionKW >= minPower.Value) &&
                (!maxPower.HasValue || r.PowerConsumptionKW <= maxPower.Value)
            )
        {
            AddInclude(r => r.Device);
            AddInclude("Device.Zone");
            ApplyOrderByDescending(r => r.Timestamp);

            if (pageSize > 0)
            {
                ApplyPaging(pageIndex * pageSize, pageSize);
            }
        }
    }
}
