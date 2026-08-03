using EnergyOptimizer.Core.Contracts;
using MediatR;
using EnergyOptimizer.Core.DTOs.ZoneDTOs;

namespace EnergyOptimizer.Core.Features.Zones.Commands
{
    public record CreateZoneCommand(CreateZoneDto Dto) : IRequest<ApiResponse>;
}
