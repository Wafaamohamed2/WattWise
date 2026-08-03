using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Alerts.Queries
{
   public record GetAlertByIdQuery (int Id) : IRequest<ApiResponse>;
   
}
