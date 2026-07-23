using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record ResendConfirmationEmailCommand(string Email) : IRequest<ApiResponse>;
}
