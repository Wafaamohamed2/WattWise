using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record DetectDeviceAnomaliesCommand(int DeviceId, int Days) : IRequest<ApiResponse>;
}
