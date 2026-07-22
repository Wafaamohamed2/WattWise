using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record RevokeTokenCommand(string RefreshToken) : IRequest<ApiResponse>;
}
