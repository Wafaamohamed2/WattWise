using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries
{
   public record ExportReadingsQuery(int? DeviceId, string? StartDate, string? EndDate) : IRequest<ApiResponse>;
}
