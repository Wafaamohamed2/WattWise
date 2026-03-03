using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AskAIQuestionQuery(string Question, string? Context) : IRequest<ApiResponse>;
}
