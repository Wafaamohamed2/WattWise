using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record ForgotPasswordCommand(string Email) : IRequest<ApiResponse>;
}
