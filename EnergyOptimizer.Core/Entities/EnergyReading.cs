namespace EnergyOptimizer.Core.Entities
{
    public class EnergyReading
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public double PowerConsumptionKW { get; set; }

        public double Voltage { get; set; } = 220;

        public double Current { get; set; }

        public double Temperature { get; set; }

        // Navigation Properties
        public virtual Device Device { get; set; } = null!;
    }
}