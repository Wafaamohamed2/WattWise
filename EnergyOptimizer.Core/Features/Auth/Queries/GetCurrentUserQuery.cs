using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Queries
{
    public record GetCurrentUserQuery(string UserId) : IRequest<ApiResponse>;
}
