using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans
{
    public record CreateReadingCommand(CreateReadingDto Dto) : IRequest<ApiResponse>;
}
