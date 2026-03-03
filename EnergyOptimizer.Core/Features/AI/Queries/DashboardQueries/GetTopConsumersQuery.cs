using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DashboardQueries
{
    public record GetTopConsumersQuery(int Count, string StartDate) : IRequest<ApiResponse>;
}
