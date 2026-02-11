using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;


namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public record DeleteDeviceCommand(int DeviceId) : IRequest<ApiResponse>;
    
}
