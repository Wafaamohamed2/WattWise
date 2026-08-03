using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.Devices.Commands
{
    public record CreateDeviceCommand(CreateDeviceDto Dto) : IRequest<ApiResponse>;
}
