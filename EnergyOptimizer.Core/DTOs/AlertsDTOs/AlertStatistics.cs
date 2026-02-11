namespace EnergyOptimizer.Core.DTOs.AlertsDTOs
{
    public class AlertStatistics
    {
        public int TotalAlerts { get; set; }
        public int UnreadAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
    }
}
