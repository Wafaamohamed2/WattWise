using MediatR;
using EnergyOptimizer.Core.DTOs.BuildingDTOs;

namespace EnergyOptimizer.Core.Features.AI.Commands.BuildingCommands
{
    public record UpdateBuildingCommand(int Id, UpdateBuildingDto Dto) : IRequest<ApiResponse>;
}
