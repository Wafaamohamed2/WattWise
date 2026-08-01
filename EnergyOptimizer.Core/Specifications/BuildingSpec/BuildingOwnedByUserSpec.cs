using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.BuildingSpec
{
    public class BuildingOwnedByUserSpec : BaseSpecifcation<Building>
    {
        public BuildingOwnedByUserSpec(string userId)
            : base(b => b.UserId == userId)
        {
            AddInclude(b => b.Zones);
        }

        public BuildingOwnedByUserSpec(int buildingId, string userId)
            : base(b => b.Id == buildingId && b.UserId == userId)
        {
            AddInclude(b => b.Zones);
        }
    }
}
