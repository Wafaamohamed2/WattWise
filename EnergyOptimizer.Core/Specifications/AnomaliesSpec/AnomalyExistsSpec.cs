using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Core.Specifications.AnomaliesSpec
{
    public class AnomalyExistsSpec : BaseSpecifcation<DetectedAnomaly>
    {
        public AnomalyExistsSpec(int deviceId, DateTime timestamp)
            : base(a => a.DeviceId == deviceId && a.AnomalyTimestamp == timestamp)
        {
        }
    }
}
