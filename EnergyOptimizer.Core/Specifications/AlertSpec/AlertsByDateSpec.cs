using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertsByDateSpec : BaseSpecifcation<Alert>
    {
        public AlertsByDateSpec(DateTime startDate, string userId)
            : base(a => a.CreatedAt >= startDate &&
                        a.Device != null && a.Device.Zone != null && a.Device.Zone.Building != null &&
                        a.Device.Zone.Building.UserId == userId)
        {
            AddInclude(a => a.Device);
            ApplyOrderByDescending(a => a.CreatedAt);
        }
    }
}
