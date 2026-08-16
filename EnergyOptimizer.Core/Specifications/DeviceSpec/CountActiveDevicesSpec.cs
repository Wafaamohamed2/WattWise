using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class CountActiveDevicesSpec : BaseSpecifcation<Device>
    {
        public CountActiveDevicesSpec(string? userId = null, bool? isActive = null, int? buildingId = null)
            : base(x => (!isActive.HasValue || x.IsActive == isActive.Value) &&
                        (userId == null || (x.Zone != null && x.Zone.Building != null && x.Zone.Building.UserId == userId)) &&
                        (!buildingId.HasValue || (x.Zone != null && x.Zone.BuildingId == buildingId.Value)))
        {
        }
    }
}
