using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.DTOs.OnboardingDTOs;
using MediatR;

namespace EnergyOptimizer.Core.Features.Onboarding.Commands
{
    public record CompleteOnboardingCommand(CompleteOnboardingDto Dto) : IRequest<ApiResponse>;
}
