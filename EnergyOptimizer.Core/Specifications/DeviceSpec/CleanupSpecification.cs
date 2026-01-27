using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.DeviceSpec
{
    public class CleanupSpecification<T> : BaseSpecifcation<T> where T : class
    {
        public CleanupSpecification(System.Linq.Expressions.Expression<Func<T, bool>> criteria) : base(criteria){
            ApplyAsNoTracking(false);
        }
    }
}
