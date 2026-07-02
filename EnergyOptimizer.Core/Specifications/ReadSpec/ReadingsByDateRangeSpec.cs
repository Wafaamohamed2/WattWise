using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.ReadSpec
{
    public class ReadingsByDateRangeSpec : BaseSpecifcation<EnergyReading>
    {
        public ReadingsByDateRangeSpec(DateTime start, DateTime end)
            : base(r => r.Timestamp >= start && r.Timestamp <= (end.TimeOfDay == TimeSpan.Zero ? end.Date.AddDays(1).AddTicks(-1) : end))
        {
            AddInclude(r => r.Device);
            AddInclude(r => r.Device.Zone);
            ApplyOrderByDescending(r => r.Timestamp);
        }
    }
}
