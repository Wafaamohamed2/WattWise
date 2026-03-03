using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
public record GetLatestReadingsQuery(int Limit, string? StartDate = null, string? EndDate = null) 
    : IRequest<ApiResponse>;
}
