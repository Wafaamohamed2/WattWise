using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class LatestReadingsSpec : BaseSpecifcation<EnergyReading>
    {
        public LatestReadingsSpec(int limit = 50)
        {
            AddInclude(r => r.Device);
            AddInclude(r => r.Device.Zone);
            ApplyOrderByDescending(r => r.Timestamp);
            ApplyPaging(0, limit);
        }

        public LatestReadingsSpec(string userId, int limit = 50, int? buildingId = null)
            : base(r => r.Device != null && r.Device.Zone != null && r.Device.Zone.Building != null && 
                        r.Device.Zone.Building.UserId == userId &&
                        (!buildingId.HasValue || r.Device.Zone.BuildingId == buildingId.Value))
        {
            AddInclude(r => r.Device);
            AddInclude(r => r.Device.Zone);
            ApplyOrderByDescending(r => r.Timestamp);
            ApplyPaging(0, limit);
        }
    }
}
