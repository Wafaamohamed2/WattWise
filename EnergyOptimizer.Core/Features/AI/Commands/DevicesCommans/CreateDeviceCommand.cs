using EnergyOptimizer.Core.DTOs.DeviceDTOs;
using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public record CreateDeviceCommand(CreateDeviceDto Dto) : IRequest<ApiResponse>;
}
