namespace EnergyOptimizer.Core.DTOs.AlertsDTOs
{
    public class AlertSettings
    {
        public int CheckIntervalMinutes { get; set; } = 1;
        public HighConsumptionSettings HighConsumptionDetection { get; set; } = new();
        public AnomalyDetectionSettings AnomalyDetection { get; set; } = new();
        public WastageSettings WastageDetection { get; set; } = new();
        public OfflineDetectionSettings OfflineDetection { get; set; } = new();
    }

    public class HighConsumptionSettings
    {
        public decimal MultiplicationFactor { get; set; } = 1.5m;
        public int MinReadingsCount { get; set; } = 5;
        public decimal MinConsumptionKW { get; set; } = 0.5m;
    }

    public class AnomalyDetectionSettings
    {
        public double ZScoreThreshold { get; set; } = 3.0;
        public int MinReadingsCount { get; set; } = 10;
        public bool ConsiderNightHours { get; set; } = true;
    }

    public class WastageSettings
    {
        public double ThresholdKW { get; set; } = 0.05;
        public int NoUseTimeRangeStart { get; set; } = 23;
        public int NoUseTimeRangeEnd { get; set; } = 6;
        public double WashingMachineThresholdKW { get; set; } = 0.5;
    }

    public class OfflineDetectionSettings
    {
        public int TimeoutMinutes { get; set; } = 10;
    }
}
