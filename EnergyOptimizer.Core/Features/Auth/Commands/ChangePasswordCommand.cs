using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record ChangePasswordCommand(string UserId, string CurrentPassword, string NewPassword) : IRequest<ApiResponse>;
}
