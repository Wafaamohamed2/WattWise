using EnergyOptimizer.Core.Contracts;
using MediatR;
using EnergyOptimizer.Core.DTOs.ZoneDTOs;

namespace EnergyOptimizer.Core.Features.Zones.Commands
{
    public record UpdateZoneCommand(int Id, UpdateZoneDto Dto) : IRequest<ApiResponse>;
}
