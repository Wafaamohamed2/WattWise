using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
  public record ToggleDeviceCommand(int Id ) : IRequest<ApiResponse>;
}
