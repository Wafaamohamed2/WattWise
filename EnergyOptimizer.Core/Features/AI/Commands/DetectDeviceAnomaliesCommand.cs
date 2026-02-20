using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record DetectDeviceAnomaliesCommand(int DeviceId, int Days) : IRequest<ApiResponse>;
}
