using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertOwnedByUserSpec : BaseSpecifcation<Alert>
    {
        public AlertOwnedByUserSpec(int alertId, string userId)
            : base(a => a.Id == alertId &&
                        a.Device != null &&
                        a.Device.Zone != null &&
                        a.Device.Zone.Building != null &&
                        a.Device.Zone.Building.UserId == userId)
        {
            AddInclude(a => a.Device);
        }
    }
}
