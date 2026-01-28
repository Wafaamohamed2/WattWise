using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Interfaces
{
    public interface IEnergyHubService
    {
        Task SendAlertNotification(string message);
    }
}
