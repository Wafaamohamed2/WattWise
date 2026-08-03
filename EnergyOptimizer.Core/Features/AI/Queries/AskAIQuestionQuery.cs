using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AskAIQuestionQuery(string Question, string? Context) : IRequest<ApiResponse>;
}
