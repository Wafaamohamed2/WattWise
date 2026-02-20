using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public record AskAIQuestionQuery(string Question, string? Context) : IRequest<ApiResponse>;
}
