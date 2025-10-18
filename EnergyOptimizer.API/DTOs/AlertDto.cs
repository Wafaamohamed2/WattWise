namespace EnergyOptimizer.API.DTOs
{
    public class AlertDto
    {
        public int Id { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public string ZoneName { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Severity { get; set; }
        public string SeverityLabel { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public string Icon { get; set; } = string.Empty;
    }
    public class AlertStatistics
    {
        public int TotalAlerts { get; set; }
        public int UnreadAlerts { get; set; }
        public int CriticalAlerts { get; set; }
        public int WarningAlerts { get; set; }
        public int InfoAlerts { get; set; }
    }
}
