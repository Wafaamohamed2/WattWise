using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Specifications.AlertSpec
{
    public class AlertCountSpec : BaseSpecifcation<Alert>
    {
        public string UserId { get; }
        public bool? IsRead { get; }
        public AlertSeverity? Severity { get; }
        public DateTime? StartDate { get; }

        public AlertCountSpec(string userId, bool? isRead = null, AlertSeverity? severity = null, DateTime? startDate = null)
            : base(x => (!startDate.HasValue || x.CreatedAt >= startDate.Value) &&
                        (!isRead.HasValue || x.IsRead == isRead.Value) &&
                        (!severity.HasValue || x.Severity == severity.Value) &&
                        x.Device != null && x.Device.Zone != null && x.Device.Zone.Building != null &&
                        x.Device.Zone.Building.UserId == userId)
        {
            UserId = userId;
            IsRead = isRead;
            Severity = severity;
            StartDate = startDate;
        }
    }
}
