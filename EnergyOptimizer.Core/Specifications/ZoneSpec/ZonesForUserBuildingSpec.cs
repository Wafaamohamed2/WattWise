using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ZoneSpec
{
    public class ZonesForUserBuildingSpec : BaseSpecifcation<Zone>
    {
        public ZonesForUserBuildingSpec(string userId)
            : base(z => z.Building.UserId == userId)
        {
            AddInclude(z => z.Devices);
        }

        public ZonesForUserBuildingSpec(int buildingId, string userId)
            : base(z => z.BuildingId == buildingId && z.Building.UserId == userId)
        {
            AddInclude(z => z.Devices);
        }
    }
}
