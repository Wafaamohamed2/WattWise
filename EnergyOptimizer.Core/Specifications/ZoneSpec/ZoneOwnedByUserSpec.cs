using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ZoneSpec
{
    public class ZoneOwnedByUserSpec : BaseSpecifcation<Zone>
    {
        public ZoneOwnedByUserSpec(int zoneId, string userId)
            : base(z => z.Id == zoneId && z.Building.UserId == userId)
        {
            AddInclude(z => z.Devices);
        }
    }
}
