namespace EnergyOptimizer.Core.Contracts
{
    public record EnergyReadingReceivedEvent(
        int DeviceId,
        decimal PowerConsumptionKW, 
        decimal Voltage,
        decimal Current,
        double Temperature,
        DateTime Timestamp
    );
}
