using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertByIdSpec : BaseSpecifcation<Alert>
    {
        public AlertByIdSpec(int id)
            : base(a => a.Id == id)
        {
            AddInclude(a => a.Device);
            AddInclude("Device.Zone");
        }
    }
}
