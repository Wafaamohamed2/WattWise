using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DevicesByZoneSpec : BaseSpecifcation<Device>
    {
        public DevicesByZoneSpec(int zoneId, string userId)
            : base(d => d.ZoneId == zoneId && d.Zone.Building.UserId == userId)
        {
            AddInclude(d => d.Zone);
            ApplyOrderByDescending(d => d.RatedPowerKW);
        }
    }
}
