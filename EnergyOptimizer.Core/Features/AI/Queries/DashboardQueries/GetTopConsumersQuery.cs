using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetTopConsumersQuery(int Count, string StartDate) : IRequest<ApiResponse>;
}
