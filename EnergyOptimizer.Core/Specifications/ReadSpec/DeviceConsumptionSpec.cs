using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class DeviceConsumptionSpec : BaseSpecifcation<EnergyReading>
    {
        public DeviceConsumptionSpec(DateTime start, DateTime end, string userId, int? buildingId = null)
            : base(r => r.Timestamp >= start && r.Timestamp <= end &&
                        r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null &&
                        r.Device.Zone.Building.UserId == userId &&
                        (!buildingId.HasValue || r.Device.Zone.BuildingId == buildingId.Value))
        {
            AddInclude(r => r.Device);
            AddInclude(r => r.Device.Zone);
        }
    }
}
