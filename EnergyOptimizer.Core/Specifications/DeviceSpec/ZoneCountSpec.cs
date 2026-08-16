using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class ZoneCountSpec : BaseSpecifcation<Zone>
    {
        public ZoneCountSpec(string? userId = null, int? buildingId = null) 
            : base(z => (userId == null || (z.Building != null && z.Building.UserId == userId)) &&
                        (!buildingId.HasValue || z.BuildingId == buildingId.Value)) 
        {
        }
    }
}
