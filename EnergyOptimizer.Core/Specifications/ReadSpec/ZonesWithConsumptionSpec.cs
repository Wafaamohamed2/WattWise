using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ZonesWithConsumptionSpec : BaseSpecifcation<Zone>
    {
        public ZonesWithConsumptionSpec()
        {
            AddInclude(z => z.Devices);
            ApplyOrderBy(z => z.Name);
        }
    }
}
