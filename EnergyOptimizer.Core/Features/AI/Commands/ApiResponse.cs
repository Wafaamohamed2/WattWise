
namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record ApiResponse(int StatusCode, string Message, object? Details = null);

}
