using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public record CreateDeviceCommand(CreateDeviceDto Dto) : IRequest<ApiResponse>;
}
