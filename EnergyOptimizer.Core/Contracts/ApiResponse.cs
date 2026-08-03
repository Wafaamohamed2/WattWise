using EnergyOptimizer.Core.Contracts;
namespace EnergyOptimizer.Core.Contracts
{
    public record ApiResponse(int StatusCode, string Message, object? Details = null);
}
