using MediatR;
using EnergyOptimizer.Core.DTOs.ZoneDTOs;

namespace EnergyOptimizer.Core.Features.AI.Commands.ZoneCommands
{
    public record UpdateZoneCommand(int Id, UpdateZoneDto Dto) : IRequest<ApiResponse>;
}
