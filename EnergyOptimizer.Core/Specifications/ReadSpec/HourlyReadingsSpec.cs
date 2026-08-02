using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class HourlyReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public HourlyReadingsSpec(DateTime date, string userId)
           : base(r => r.Timestamp >= date.Date &&
                       r.Timestamp < date.Date.AddDays(1) &&
                       r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null &&
                       r.Device.Zone.Building.UserId == userId)
        {
            AddInclude(r => r.Device);
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
