using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public record RunGlobalAnalysisCommand : IRequest<ApiResponse>;
}
