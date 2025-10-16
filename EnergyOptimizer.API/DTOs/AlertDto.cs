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
    }
}
