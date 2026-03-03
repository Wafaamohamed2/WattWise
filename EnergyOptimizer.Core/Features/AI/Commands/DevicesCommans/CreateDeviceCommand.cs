using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public record CreateDeviceCommand(CreateDeviceDto Dto) : IRequest<ApiResponse>;
}
