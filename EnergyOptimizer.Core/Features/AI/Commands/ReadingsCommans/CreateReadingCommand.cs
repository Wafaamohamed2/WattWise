using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans
{
    public record CreateReadingCommand(CreateReadingDto Dto) : IRequest<ApiResponse>;
}
