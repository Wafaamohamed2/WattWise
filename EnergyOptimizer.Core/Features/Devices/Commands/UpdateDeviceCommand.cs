using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.Devices.Commands
{
    public record UpdateDeviceCommand(int id ,UpdateDeviceDto Dto) : IRequest<ApiResponse>;
}
