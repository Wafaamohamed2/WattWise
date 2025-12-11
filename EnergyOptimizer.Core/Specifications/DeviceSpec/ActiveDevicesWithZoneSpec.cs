using EnergyOptimizer.Core.Entities;


namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class ActiveDevicesWithZoneSpec : BaseSpecifcation<Device>
    {
        public ActiveDevicesWithZoneSpec()
            : base(d => d.IsActive)
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
        }
    }

}
