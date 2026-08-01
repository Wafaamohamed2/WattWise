using MediatR;
using EnergyOptimizer.Core.DTOs.BuildingDTOs;

namespace EnergyOptimizer.Core.Features.AI.Commands.BuildingCommands
{
    public record CreateBuildingCommand(CreateBuildingDto Dto) : IRequest<ApiResponse>;
}
