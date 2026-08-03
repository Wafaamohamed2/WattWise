using EnergyOptimizer.Core.Contracts;
using MediatR;


namespace EnergyOptimizer.Core.Features.Devices.Commands
{
    public record DeleteDeviceCommand(int DeviceId) : IRequest<ApiResponse>;
    
}
