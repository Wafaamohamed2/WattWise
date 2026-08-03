using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record RefreshTokenCommand(string RefreshToken, string? IpAddress = null) : IRequest<ApiResponse>;
}
