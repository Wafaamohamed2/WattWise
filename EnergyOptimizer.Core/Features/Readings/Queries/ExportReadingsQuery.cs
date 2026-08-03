using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Readings.Queries
{
   public record ExportReadingsQuery(int? DeviceId, string? StartDate, string? EndDate) : IRequest<ApiResponse>;
}
