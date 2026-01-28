using EnergyOptimizer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class AlertOfflineCheckSpec : BaseSpecifcation<Alert>
    {
        public AlertOfflineCheckSpec(int deviceId, DateTime since)
        : base(a => a.DeviceId == deviceId &&
                    a.Type == Core.Enums.AlertType.DeviceOffline &&
                    a.CreatedAt >= since)
        {
        }
    }
}
