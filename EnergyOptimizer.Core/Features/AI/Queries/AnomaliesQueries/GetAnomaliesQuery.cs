using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries
{
    public record GetAnomaliesQuery(bool? IsResolved, string? Severity, int? DeviceId, int Page, int PageSize) : IRequest<ApiResponse>;
}
