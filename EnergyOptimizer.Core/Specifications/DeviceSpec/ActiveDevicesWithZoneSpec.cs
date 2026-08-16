using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class ActiveDevicesWithZoneSpec : BaseSpecifcation<Device>
    {
        public ActiveDevicesWithZoneSpec(bool? isActive, string userId, int? buildingId = null)
            : base(d => (!isActive.HasValue || d.IsActive == isActive.Value) &&
                        d.Zone != null && d.Zone.Building != null && d.Zone.Building.UserId == userId &&
                        (!buildingId.HasValue || d.Zone.BuildingId == buildingId.Value))
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
        }
    }
}
