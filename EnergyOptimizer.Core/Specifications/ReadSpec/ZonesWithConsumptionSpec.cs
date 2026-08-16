using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ZonesWithConsumptionSpec : BaseSpecifcation<Zone>
    {
        public ZonesWithConsumptionSpec(string? userId = null, int? buildingId = null)
            : base(z => (userId == null || (z.Building != null && z.Building.UserId == userId)) &&
                        (!buildingId.HasValue || z.BuildingId == buildingId.Value))
        {
            AddInclude(z => z.Devices);
            ApplyOrderBy(z => z.Name);
        }
    }
}
