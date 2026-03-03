using EnergyOptimizer.Core.Entities;


namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class ActiveDevicesWithZoneSpec : BaseSpecifcation<Device>
    {
        public ActiveDevicesWithZoneSpec(bool? isActive)
            : base(d => !isActive.HasValue || d.IsActive == isActive.Value)
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
        }
    }

}
