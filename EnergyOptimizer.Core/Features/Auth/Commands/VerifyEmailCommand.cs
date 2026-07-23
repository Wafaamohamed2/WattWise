using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record VerifyEmailCommand(string UserId, string Token) : IRequest<ApiResponse>;
}
