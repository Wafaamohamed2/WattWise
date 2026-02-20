using MediatR;
using static EnergyOptimizer.Core.Features.AI.Commands.Middleware.ExceptionMiddleware;

namespace EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries
{
   public record GetAlertByIdQuery (int Id) : IRequest<ApiResponse>;
   
}
