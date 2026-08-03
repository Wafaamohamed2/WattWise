using MediatR;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Devices.Queries
{
   public record GetDeviceByIdQuery(int Id) : IRequest<ApiResponse>;
}
