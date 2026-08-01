using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class DeviceWithDetailsSpec : BaseSpecifcation<Device>
    {
        public DeviceWithDetailsSpec(int deviceId, string userId)
            : base(d => d.Id == deviceId && d.Zone != null && d.Zone.Building != null && d.Zone.Building.UserId == userId)
        {
            AddInclude(d => d.Zone);
            AddInclude(d => d.EnergyReadings);
        }
    }
}
