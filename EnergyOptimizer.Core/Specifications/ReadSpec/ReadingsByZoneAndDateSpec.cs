using EnergyOptimizer.Core.Entities;

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
                       r.Timestamp <= (endDate.TimeOfDay == TimeSpan.Zero ? endDate.Date.AddDays(1).AddTicks(-1) : endDate))
        {
            AddInclude(r => r.Device);
            AddInclude("Device.Zone");
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
