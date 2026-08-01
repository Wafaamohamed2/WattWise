using MediatR;
using EnergyOptimizer.Core.DTOs.ZoneDTOs;

namespace EnergyOptimizer.Core.Features.AI.Commands.ZoneCommands
{
    public record CreateZoneCommand(CreateZoneDto Dto) : IRequest<ApiResponse>;
}
