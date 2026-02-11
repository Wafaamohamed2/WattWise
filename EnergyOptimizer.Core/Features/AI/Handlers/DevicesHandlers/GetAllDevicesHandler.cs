using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries.DevicesQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Core.Features.AI.Handlers.DevicesHandlers
{
    public class GetAllDevicesHandler : IRequestHandler<GetAllDevicesQuery, ApiResponse>
    {

        private readonly IGenericRepository<Device> _deviceRepo;

        public GetAllDevicesHandler(IGenericRepository<Device> deviceRepo)
        {
            _deviceRepo = deviceRepo;
        }

        public async Task<ApiResponse> Handle(GetAllDevicesQuery request, CancellationToken ct)
        {
            var spec = new ActiveDevicesWithZoneSpec(request.IsActive);
            var devices = await _deviceRepo.ListAsync(spec);

            return new ApiResponse(200, "Devices retrieved successfully", devices);
        }
    }
}
