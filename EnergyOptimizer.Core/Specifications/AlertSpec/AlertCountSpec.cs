using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertCountSpec : BaseSpecifcation<Alert>
    {
        public AlertCountSpec(bool? isRead = null)
            : base(x => !isRead.HasValue || x.IsRead == isRead.Value)
        {
        }
    }
}
