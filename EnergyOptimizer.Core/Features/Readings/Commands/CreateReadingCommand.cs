using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.Readings.Commands
{
    public record CreateReadingCommand(CreateReadingDto Dto) : IRequest<ApiResponse>;
}
