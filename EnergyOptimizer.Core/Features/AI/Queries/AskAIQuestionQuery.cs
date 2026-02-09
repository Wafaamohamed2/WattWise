using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AskAIQuestionQuery(string Question, string? Context) : IRequest<ApiResponse>;
}
