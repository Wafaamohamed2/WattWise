using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.Onboarding.Queries;
using EnergyOptimizer.Core.Specifications.DeviceTemplateSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Onboarding.Handlers
{
    public class GetDeviceTemplatesHandler : IRequestHandler<GetDeviceTemplatesQuery, ApiResponse>
    {
        private readonly IGenericRepository<DeviceTemplate> _templateRepo;

        public GetDeviceTemplatesHandler(IGenericRepository<DeviceTemplate> templateRepo)
        {
            _templateRepo = templateRepo;
        }

        public async Task<ApiResponse> Handle(GetDeviceTemplatesQuery request, CancellationToken ct)
        {
            var spec = new DeviceTemplateByBuildingTypeSpec(request.BuildingType);
            var templates = await _templateRepo.ListAsync(spec);

            return new ApiResponse(200, "Device templates retrieved successfully", templates);
        }
    }
}
