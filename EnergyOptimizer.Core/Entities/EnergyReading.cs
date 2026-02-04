namespace EnergyOptimizer.Core.Entities
{
    public class EnergyReading
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public decimal PowerConsumptionKW { get; set; }

        public decimal Voltage { get; set; } = 220;

        public decimal Current { get; set; }

        public double Temperature { get; set; }

        // Navigation Properties
        public virtual Device Device { get; set; } = null!;
    }
}