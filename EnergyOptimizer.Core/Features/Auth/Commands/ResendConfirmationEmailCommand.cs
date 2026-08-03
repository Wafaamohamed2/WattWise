using EnergyOptimizer.Core.Contracts;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record ResendConfirmationEmailCommand(string Email) : IRequest<ApiResponse>;
}
