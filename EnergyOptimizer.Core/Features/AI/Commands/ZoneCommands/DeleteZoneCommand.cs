using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands.ZoneCommands
{
    public record DeleteZoneCommand(int ZoneId) : IRequest<ApiResponse>;
}
