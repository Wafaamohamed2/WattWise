using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class AllActiveDevicesForSystemSpec : BaseSpecifcation<Device>
    {
        public AllActiveDevicesForSystemSpec(bool? isActive = true)
            : base(d => !isActive.HasValue || d.IsActive == isActive.Value)
        {
            AddInclude(d => d.Zone);
            ApplyOrderBy(d => d.Name);
        }
    }
}
