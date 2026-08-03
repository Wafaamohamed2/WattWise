using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Devices.Commands
{
  public record ToggleDeviceCommand(int Id ) : IRequest<ApiResponse>;
}
