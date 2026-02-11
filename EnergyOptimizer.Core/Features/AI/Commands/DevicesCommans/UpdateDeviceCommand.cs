using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public record UpdateDeviceCommand(UpdateDeviceDto Dto) : IRequest<ApiResponse>;
}
