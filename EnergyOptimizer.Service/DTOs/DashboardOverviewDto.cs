namespace EnergyOptimizer.API.DTOs
{
    public class DashboardOverviewDto
    {
        public int TotalDevices { get; set; }
        public int ActiveDevices { get; set; }
        public int InactiveDevices { get; set; }
        public int TotalZones { get; set; }
        public double CurrentTotalConsumption { get; set; }
        public decimal TodayTotalConsumption { get; set; }
        public decimal AverageConsumptionPerHour { get; set; }
        public int TotalReadingsToday { get; set; }
        public DateTime LastReadingTime { get; set; }

    }
}
