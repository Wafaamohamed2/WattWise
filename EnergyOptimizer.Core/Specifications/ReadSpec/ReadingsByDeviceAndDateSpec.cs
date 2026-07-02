using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ReadingsByDeviceAndDateSpec : BaseSpecifcation<EnergyReading>
    {
        public ReadingsByDeviceAndDateSpec(
           int deviceId,
           DateTime startDate,
           DateTime endDate)
           : base(r => r.DeviceId == deviceId &&
                       r.Timestamp >= startDate &&
                       r.Timestamp <= (endDate.TimeOfDay == TimeSpan.Zero ? endDate.Date.AddDays(1).AddTicks(-1) : endDate))
        {
            AddInclude(r => r.Device);
            ApplyOrderBy(r => r.Timestamp);
        }
    }
}
