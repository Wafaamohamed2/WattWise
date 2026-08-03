using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Zones.Commands
{
    public record DeleteZoneCommand(int ZoneId) : IRequest<ApiResponse>;
}
