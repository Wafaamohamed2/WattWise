using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<ApiResponse>;
}
