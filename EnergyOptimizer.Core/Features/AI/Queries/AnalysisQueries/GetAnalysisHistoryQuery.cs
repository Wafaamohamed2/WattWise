using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries
{
    public record GetAnalysisHistoryQuery(int Page, int PageSize, string? AnalysisType, DateTime? StartDate, DateTime? EndDate) : IRequest<ApiResponse>;
}

