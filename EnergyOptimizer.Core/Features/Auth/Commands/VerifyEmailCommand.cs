using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record VerifyEmailCommand(string UserId, string Token) : IRequest<ApiResponse>;
}
