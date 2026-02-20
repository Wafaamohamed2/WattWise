using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
  public record ToggleDeviceCommand(int Id ) : IRequest<ApiResponse>;
}
