using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries
{
   public record GetDeviceByIdQuery(int Id) : IRequest<ApiResponse>;
}
