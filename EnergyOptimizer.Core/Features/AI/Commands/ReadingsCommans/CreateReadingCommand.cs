using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.ReadingsCommans
{
    public record CreateReadingCommand(CreateReadingDto Dto) : IRequest<ApiResponse>;
}
