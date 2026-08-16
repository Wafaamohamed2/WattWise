using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Enums;
using MediatR;

namespace EnergyOptimizer.Core.Features.Onboarding.Queries
{
    public record GetDeviceTemplatesQuery(BuildingType BuildingType) : IRequest<ApiResponse>;
}
