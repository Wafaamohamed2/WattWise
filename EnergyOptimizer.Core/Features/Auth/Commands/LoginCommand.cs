using EnergyOptimizer.Core.Features.AI.Commands;
using MediatR;
using static EnergyOptimizer.Core.DTOs.AuthDto;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public record LoginCommand(LoginDto Dto, string? IpAddress = null) : IRequest<ApiResponse>;
}
