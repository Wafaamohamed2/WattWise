using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Specifications.DeviceSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers
{
    public class GetDeviceReadingsHandler : IRequestHandler<GetDeviceReadingsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public GetDeviceReadingsHandler(
            IGenericRepository<EnergyReading> readingRepo,
            IGenericRepository<Device> deviceRepo,
            ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetDeviceReadingsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            var deviceSpec = new DeviceWithDetailsSpec(request.DeviceId, userId);
            var deviceExists = await _deviceRepo.AnyAsync(deviceSpec);
            if (!deviceExists)
                throw new NotFoundException($"Device with ID {request.DeviceId} not found");

            DateTime start = string.IsNullOrEmpty(request.StartDate)
                ? DateTime.UtcNow.AddDays(-7).Date
                : DateTime.Parse(request.StartDate);
            DateTime end = string.IsNullOrEmpty(request.EndDate)
                ? DateTime.UtcNow
                : DateTime.Parse(request.EndDate);

            var spec = new ReadingsByDeviceAndDateSpec(request.DeviceId, start, end, userId);
            var readings = await _readingRepo.ListAsync(spec);

            return new ApiResponse(200, "Device readings retrieved successfully", readings.Take(request.Limit));
        }
    }
}
