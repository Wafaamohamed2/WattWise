using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Buildings.Queries
{
    public record GetUserBuildingsQuery : IRequest<ApiResponse>;
}
