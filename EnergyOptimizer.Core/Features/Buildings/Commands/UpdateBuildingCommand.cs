using EnergyOptimizer.Core.Contracts;
using MediatR;
using EnergyOptimizer.Core.DTOs.BuildingDTOs;

namespace EnergyOptimizer.Core.Features.Buildings.Commands
{
    public record UpdateBuildingCommand(int Id, UpdateBuildingDto Dto) : IRequest<ApiResponse>;
}
