using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<ApiResponse>;
}
