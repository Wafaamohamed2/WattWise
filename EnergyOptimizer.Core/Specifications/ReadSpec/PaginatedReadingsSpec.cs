using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class PaginatedReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public PaginatedReadingsSpec(
            DateTime startDate,
            DateTime endDate,
            int? deviceId = null,
            int? zoneId = null,
            decimal? minPower = null,
            decimal? maxPower = null,
            int pageIndex = 0,
            int pageSize = 50)
            : base(r =>
                // Mandatory Date Range
                r.Timestamp >= startDate && r.Timestamp <= (endDate.TimeOfDay == TimeSpan.Zero ? endDate.Date.AddDays(1).AddTicks(-1) : endDate) &&

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
