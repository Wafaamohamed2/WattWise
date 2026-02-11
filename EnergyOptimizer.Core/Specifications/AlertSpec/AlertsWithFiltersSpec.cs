using EnergyOptimizer.Core.Entities;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertsWithFiltersSpec : BaseSpecifcation<Alert>
    {
        public AlertsWithFiltersSpec(bool? isRead, int? severity, int? deviceId, DateTime start, DateTime end, int? page = null, int? pageSize = null)
            : base(a => (a.CreatedAt >= start && a.CreatedAt <= end) &&
                        (!isRead.HasValue || a.IsRead == isRead.Value) &&
                        (!severity.HasValue || a.Severity == severity.Value) &&
                        (!deviceId.HasValue || a.DeviceId == deviceId.Value))
        {
            AddInclude(a => a.Device);
            AddInclude("Device.Zone");

            ApplyOrderByDescending(a => a.CreatedAt);

            if (page.HasValue && pageSize.HasValue)
            {
                ApplyPaging((page.Value - 1) * pageSize.Value, pageSize.Value);
            }
        }
    }
}